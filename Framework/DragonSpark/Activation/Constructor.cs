using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	[Synchronized]
	public class Constructor : ConstructorBase
	{
		readonly Func<ConstructTypeRequest, Func<object>> source;

		public static Constructor Instance { get; } = new Constructor();

		Constructor() : this( ActivatorFactory.Instance.Create ) {}

		Constructor( Func<ConstructTypeRequest, Func<object>> source ) : base( new Specification( source ) )
		{
			this.source = source;
		}

		protected override object CreateItem( ConstructTypeRequest parameter )
		{
			var factory = source( parameter );
			var result = factory();
			return result;
		}

		class Specification : SpecificationBase<ConstructTypeRequest>
		{
			readonly Func<ConstructTypeRequest, Func<object>> source;

			// public Specification() : this( ActivatorFactory.Instance.Create ) {}

			public Specification( Func<ConstructTypeRequest, Func<object>> source )
			{
				this.source = source;
			}

			protected override bool Verify( ConstructTypeRequest parameter )
			{
				var result = source( parameter ) != null;
				return result;
			}
		}

		class ArgumentsFactory : FactoryBase<ConstructTypeRequest, object[]>
		{
			public static ArgumentsFactory Instance { get; } = new ArgumentsFactory();

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

		class ActivatorFactory : FactoryBase<ConstructTypeRequest, Func<object>>
		{
			public static ActivatorFactory Instance { get; } = new ActivatorFactory();

			readonly Func<ConstructTypeRequest, object[]> arguments;

			ActivatorFactory() : this( ArgumentsFactory.Instance.Create ) {}

			public ActivatorFactory( Func<ConstructTypeRequest, object[]> arguments )
			{
				this.arguments = arguments;
			}

			[Freeze]
			protected override Func<object> CreateItem( ConstructTypeRequest parameter )
			{
				var result = parameter
									.RequestedType
									.Adapt().With( adapter => 
										new[] { arguments( parameter ), new object[0] }
											.WithFirst( 
												objects => adapter.FindConstructor( objects ).With( GetActivator ).With( activator => new Func<object>( () => activator( objects ) ) )
												)
										);
				return result;
			}

			static ObjectActivator GetActivator( ConstructorInfo ctor )
			{
				var paramsInfo = ctor.GetParameters();
				var param = Expression.Parameter( typeof(object[]), "args" );

				var argsExp = new Expression[paramsInfo.Length];
				for ( var i = 0; i < paramsInfo.Length; i++ )
				{
					var paramAccessorExp = Expression.ArrayIndex( param, Expression.Constant( i ) );
					argsExp[i] = Expression.Convert( paramAccessorExp, paramsInfo[i].ParameterType );
				}

				var newExp = Expression.New( ctor, argsExp );

				var lambda = Expression.Lambda( typeof(ObjectActivator), newExp, param );

				//compile it
				var compiled = (ObjectActivator)lambda.Compile();
				return compiled;
			}
		}

		delegate object ObjectActivator( params object[] args );
	}
}