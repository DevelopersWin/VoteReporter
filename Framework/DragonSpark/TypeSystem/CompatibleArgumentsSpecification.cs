using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class CompatibleArgumentsSpecification : SpecificationCache<MethodBase, ImmutableArray<Type>>
	{
		public static CompatibleArgumentsSpecification Default { get; } = new CompatibleArgumentsSpecification();
		CompatibleArgumentsSpecification() : base( method => new DelegatedSpecification<ImmutableArray<Type>>( new ExtendedDictionaryCache<ImmutableArray<Type>, bool>( new DefaultImplementation( method ).IsSatisfiedBy ).Get ) ) {}

		sealed class DefaultImplementation : SpecificationWithContextBase<ImmutableArray<Parameter>, ImmutableArray<Type>>
		{
			readonly static Func<ValueTuple<Parameter, ImmutableArray<Type>>, int, bool> SelectCompatible = Compatible;

			readonly int required;

			public DefaultImplementation( MethodBase method ) : this( method.GetParameters().Select( info => new Parameter( info.ParameterType, info.IsOptional ) ).ToImmutableArray() ) {}

			DefaultImplementation( ImmutableArray<Parameter> parameters ) : this( parameters, parameters.Count( info => !info.Optional ) ) {}

			DefaultImplementation( ImmutableArray<Parameter> parameters, int required ) : base( parameters )
			{
				this.required = required;
			}

			public override bool IsSatisfiedBy( ImmutableArray<Type> parameter ) =>
				parameter.Length >= required && parameter.Length <= Context.Length
				&&
				Context.Introduce( parameter ).Select( SelectCompatible ).All();
			static bool Compatible( ValueTuple<Parameter, ImmutableArray<Type>> context, int i )
			{
				var type = context.Item2.ElementAtOrDefault( i );
				var result = type != null ? context.Item1.ParameterType.IsAssignableFrom( type ) : i < context.Item2.Length || context.Item1.Optional;
				return result;
			}
		}

		public struct Parameter
		{
			public Parameter( Type parameterType, bool optional )
			{
				ParameterType = parameterType;
				Optional = optional;
			}

			public Type ParameterType { get; }
			public bool Optional { get; }
		}
	}
}