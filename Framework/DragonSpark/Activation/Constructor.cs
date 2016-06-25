using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	[AutoValidation.GenericFactory]
	public class Constructor : ConstructorBase
	{
		readonly Func<ConstructTypeRequest, ConstructorInfo> constructorSource;
		readonly Func<ConstructorInfo, ObjectActivator> activatorSource;

		public static Constructor Instance { get; } = new Constructor();

		Constructor() : this( ArgumentCache.Instance.Get, ObjectActivatorFactory.Instance.ToDelegate() ) {}

		Constructor( Func<ConstructTypeRequest, ConstructorInfo> constructorSource, Func<ConstructorInfo, ObjectActivator> activatorSource ) : base( Specification.Instance )
		{
			this.constructorSource = constructorSource;
			this.activatorSource = activatorSource;
		}

		public T Create<T>( ConstructTypeRequest parameter ) => (T)Create( parameter );

		public override object Create( ConstructTypeRequest parameter ) => LocateAndCreate( parameter ) ?? DefaultValueFactory.Instance.Create( parameter.RequestedType );

		object LocateAndCreate( ConstructTypeRequest parameter )
		{
			var info = constructorSource( parameter );
			var result = info != null ? activatorSource( info )?.Invoke( WithOptional( parameter.Arguments, info.GetParameters() ) ) : null;
			return result;
		}

		static object[] WithOptional( IReadOnlyCollection<object> arguments, IEnumerable<ParameterInfo> parameters )
		{
			var optional = parameters.Skip( arguments.Count ).Where( info => info.IsOptional ).Select( info => info.DefaultValue );
			var result = arguments.Concat( optional ).Fixed();
			return result;
		}

		class Specification : GuardedSpecificationBase<ConstructTypeRequest>
		{
			public static Specification Instance { get; } = new Specification();

			readonly ArgumentCache cache;

			Specification() : this( ArgumentCache.Instance ) {}
			Specification( ArgumentCache cache ) : base( Coercer.Instance.ToDelegate() )
			{
				this.cache = cache;
			}

			public override bool IsSatisfiedBy( ConstructTypeRequest parameter ) => parameter.RequestedType.GetTypeInfo().IsValueType || cache.Get( parameter ) != null;
		}

		internal class ArgumentCache : ProjectedCache<ConstructTypeRequest, ConstructorInfo>
		{
			public static ArgumentCache Instance { get; } = new ArgumentCache();
			ArgumentCache() : base( Determine ) {}

			static ConstructorInfo Determine( ConstructTypeRequest parameter )
			{
				var candidates = new[] { parameter.Arguments, parameter.Arguments.WhereAssigned().Fixed(), Items<object>.Default };
				var adapter = parameter.RequestedType.Adapt();
				var result = candidates
					.Introduce( adapter )
					.Select( tuple => tuple.Item2.FindConstructor( tuple.Item1 )  )
					.FirstOrDefault();
				return result;
			}
		}

		class ObjectActivatorFactory : Cache<ConstructorInfo, ObjectActivator>
		{
			public static ObjectActivatorFactory Instance { get; } = new ObjectActivatorFactory();
			ObjectActivatorFactory() : base( Create ) {}

			static ObjectActivator Create( ConstructorInfo parameter )
			{
				var parameters = parameter.GetParameterTypes();
				var param = Expression.Parameter( typeof(object[]), "args" );

				var array = new Expression[parameters.Length];
				for ( var i = 0; i < parameters.Length; i++ )
				{
					var index = Expression.ArrayIndex( param, Expression.Constant( i ) );
					array[i] = Expression.Convert( index, parameters[i] );
				}

				var create = Expression.New( parameter, array );

				var result = Expression.Lambda<ObjectActivator>( create, param ).Compile();
				return result;
			}
		}

		delegate object ObjectActivator( params object[] args );
	}
}