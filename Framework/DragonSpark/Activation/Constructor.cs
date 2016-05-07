using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
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

		protected override object CreateItem( ConstructTypeRequest parameter )
		{
			var result = LocateAndCreate( parameter ) ?? DefaultItemProvider.Instance.Create( parameter.RequestedType );
			return result;
		}

		object LocateAndCreate( ConstructTypeRequest parameter ) => constructorSource( parameter ).With( activatorSource ).With( activator => activator( parameter.Arguments ) );

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

		public class Locator  : FactoryBase<ConstructTypeRequest, ConstructorInfo>
		{
			public static Locator Instance { get; } = new Locator();

			[Freeze]
			protected override ConstructorInfo CreateItem( ConstructTypeRequest parameter )
			{
				var candidates = new[] { parameter.Arguments, parameter.Arguments.NotNull(), Default<object>.Items };
				var adapter = parameter.RequestedType.Adapt();
				var result = candidates
					.Select( objects => objects.Fixed() )
					.Select( objects => adapter.FindConstructor( objects ) )
					.FirstOrDefault();
				return result;
			}
		}

		/*class ArgumentsFactory : FactoryBase<ConstructTypeRequest, object[]>
		{
			public static ArgumentsFactory Instance { get; } = new ArgumentsFactory();

			ArgumentsFactory() : base( ConstructCoercer<ConstructTypeRequest>.Instance ) {}

			// [Freeze]
			protected override object[] CreateItem( ConstructTypeRequest parameter )
			{
				var candidates = new[] { parameter.Arguments, parameter.Arguments.NotNull() };
				var adapter = parameter.RequestedType.Adapt();
				var result = candidates
					.Select( objects => objects.Fixed() )
					.Select( objects => new { arguments = objects, constructor = adapter.FindConstructor( objects ) } )
					.Where( arg => arg.constructor != null )
					.Select( arg => Ensure( arg.constructor.GetParameters(), arg.arguments ) )
					.FirstOrDefault();
				return result;
			}

			static object[] Ensure( IEnumerable<ParameterInfo> parameters, IReadOnlyCollection<object> arguments )
			{
				var optional = parameters.Skip( arguments.Count ).Where( info => info.IsOptional ).Select( info => info.DefaultValue );
				var result = arguments.Concat( optional ).Fixed();
				return result;
			}
		}*/

		class ObjectActivatorFactory : FactoryBase<ConstructorInfo, ObjectActivator>
		{
			public static ObjectActivatorFactory Instance { get; } = new ObjectActivatorFactory();

			[Freeze]
			protected override ObjectActivator CreateItem( ConstructorInfo parameter )
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