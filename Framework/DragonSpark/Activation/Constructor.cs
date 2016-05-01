using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	public class Constructor : ConstructorBase
	{
		readonly Func<ConstructTypeRequest, Func<object>> source;

		public static Constructor Instance { get; } = new Constructor();

		Constructor() : this( ConstructionActivatorFactory.Instance.Create ) {}

		Constructor( Func<ConstructTypeRequest, Func<object>> source ) : base( new Specification( source ) )
		{
			this.source = source;
		}

		protected override object CreateItem( ConstructTypeRequest parameter )
		{
			var activator = source( parameter );
			var result = activator?.Invoke() ?? DefaultItemProvider.Instance.Create( parameter.RequestedType );
			return result;
		}

		class Specification : CoercedSpecificationBase<ConstructTypeRequest>
		{
			readonly Func<ConstructTypeRequest, Func<object>> source;

			// public Specification() : this( ActivatorFactory.Instance.Create ) {}

			public Specification( Func<ConstructTypeRequest, Func<object>> source ) : base( Coercer.Instance.Coerce )
			{
				this.source = source;
			}

			public override bool IsSatisfiedBy( ConstructTypeRequest parameter ) => parameter.RequestedType.GetTypeInfo().IsValueType || source( parameter ) != null;
		}

		class ArgumentsFactory : FactoryBase<ConstructTypeRequest, object[]>
		{
			public static ArgumentsFactory Instance { get; } = new ArgumentsFactory();

			ArgumentsFactory() : base( ConstructCoercer<ConstructTypeRequest>.Instance ) {}

			[Freeze]
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
		}

		internal class ConstructionActivatorFactory : FactoryBase<ConstructTypeRequest, Func<object>>
		{
			public static ConstructionActivatorFactory Instance { get; } = new ConstructionActivatorFactory();

			readonly Func<ConstructTypeRequest, object[]> arguments;

			ConstructionActivatorFactory() : this( ArgumentsFactory.Instance.Create ) {}

			public ConstructionActivatorFactory( Func<ConstructTypeRequest, object[]> arguments )
			{
				this.arguments = arguments;
			}

			[Freeze]
			protected override Func<object> CreateItem( ConstructTypeRequest parameter )
			{
				var result = parameter.RequestedType.Adapt().With( adapter => new[] { arguments( parameter ), Default<object>.Items }
																						.WithFirst( objects => adapter
																												.FindConstructor( objects )
																												.With( GetActivator )
																												.With( activator => new Func<object>( () => activator( objects ) ) )
												)
										);

				return result;
			}

			static ObjectActivator GetActivator( ConstructorInfo constructor )
			{
				var parameters = constructor.GetParameters();
				var param = Expression.Parameter( typeof(object[]), "args" );

				var array = new Expression[parameters.Length];
				for ( var i = 0; i < parameters.Length; i++ )
				{
					var index = Expression.ArrayIndex( param, Expression.Constant( i ) );
					array[i] = Expression.Convert( index, parameters[i].ParameterType );
				}

				var create = Expression.New( constructor, array );

				var result = Expression.Lambda<ObjectActivator>( create, param ).Compile();
				return result;
			}
		}

		delegate object ObjectActivator( params object[] args );
	}
}