using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	public class Constructor : ConstructorBase
	{
		readonly Func<ConstructTypeRequest, ConstructorInfo> constructorSource;
		readonly Func<ConstructorInfo, ObjectActivator> activatorSource;

		public static Constructor Instance { get; } = new Constructor();

		Constructor() : this( Locator.Instance.Create, ObjectActivatorFactory.Instance.Create ) {}

		Constructor( Func<ConstructTypeRequest, ConstructorInfo> constructorSource, Func<ConstructorInfo, ObjectActivator> activatorSource ) : base( Specification.Instance )
		{
			this.constructorSource = constructorSource;
			this.activatorSource = activatorSource;
		}

		public override object Create( ConstructTypeRequest parameter ) => LocateAndCreate( parameter ) ?? DefaultValueFactory.Instance.Create( parameter.RequestedType );

		object LocateAndCreate( ConstructTypeRequest parameter )
		{
			var info = constructorSource( parameter );
			if ( info != null )
			{
				var activator = activatorSource( info );
				if ( activator != null )
				{
					var result = activator( Ensure( info.GetParameters(), parameter.Arguments ) );
					return result;
				}
			}
			return null;
		}

		static object[] Ensure( IEnumerable<ParameterInfo> parameters, IReadOnlyCollection<object> arguments )
		{
			var optional = parameters.Skip( arguments.Count ).Where( info => info.IsOptional ).Select( info => info.DefaultValue );
			var result = EnumerableExtensions.Fixed( arguments.Concat( optional ) );
			return result;
		}

		class Specification : GuardedSpecificationBase<ConstructTypeRequest>
		{
			public static Specification Instance { get; } = new Specification();

			readonly Func<ConstructTypeRequest, ConstructorInfo> source;

			// public Specification() : this( ActivatorFactory.Instance.Create ) {}

			Specification() : this( Locator.Instance.Create ) {}
			Specification( Func<ConstructTypeRequest, ConstructorInfo> source ) : base( Coercer.Instance.Coerce )
			{
				this.source = source;
			}

			public override bool IsSatisfiedBy( ConstructTypeRequest parameter ) => parameter.RequestedType.GetTypeInfo().IsValueType || source( parameter ) != null;
		}

		//[AutoValidation( false )]
		public class Locator  : FactoryBase<ConstructTypeRequest, ConstructorInfo>
		{
			public static Locator Instance { get; } = new Locator();

			[Freeze]
			public override ConstructorInfo Create( ConstructTypeRequest parameter )
			{
				var candidates = ImmutableArray.Create( parameter.Arguments, parameter.Arguments.NotNull(), Items<object>.Default );
				var adapter = parameter.RequestedType.Adapt();
				var result = candidates
					.Select( objects => EnumerableExtensions.Fixed( objects ) )
					.Select( adapter.FindConstructor )
					.FirstOrDefault();
				return result;
			}
		}

		// [AutoValidation( false )]
		class ObjectActivatorFactory : FactoryBase<ConstructorInfo, ObjectActivator>
		{
			public static ObjectActivatorFactory Instance { get; } = new ObjectActivatorFactory();

			[Freeze]
			public override ObjectActivator Create( ConstructorInfo parameter )
			{
				var parameters = parameter.GetParameters();
				var param = Expression.Parameter( typeof(object[]), "args" );

				var array = new Expression[parameters.Length];
				for ( var i = 0; i < parameters.Length; i++ )
				{
					var index = Expression.ArrayIndex( param, Expression.Constant( i ) );
					array[i] = Expression.Convert( index, parameters[i].ParameterType );
				}

				var create = Expression.New( parameter, array );

				var result = Expression.Lambda<ObjectActivator>( create, param ).Compile();
				return result;
			}
		}

		delegate object ObjectActivator( params object[] args );
	}
}