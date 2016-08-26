using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ConventionImplementedInterfaces : FactoryCache<Type, Type>
	{
		public static ConventionImplementedInterfaces Default { get; } = new ConventionImplementedInterfaces();
		ConventionImplementedInterfaces() : this( typeof(ISource), typeof(IParameterizedSource), typeof(IValidatedParameterizedSource) ) {}

		readonly ImmutableArray<Type> exempt;

		public ConventionImplementedInterfaces( params Type[] exempt )
		{
			this.exempt = exempt.ToImmutableArray();
		}

		protected override Type Create( Type parameter )
		{
			foreach ( var @interface in parameter.GetTypeInfo().ImplementedInterfaces.Except( exempt.ToArray() ).ToArray() )
			{
				var specification = IsConventionCandidateSpecification.Defaults.Get( @interface );
				if ( specification( parameter ) )
				{
					return @interface;
				}
			}
			return null;
		}
	}
}