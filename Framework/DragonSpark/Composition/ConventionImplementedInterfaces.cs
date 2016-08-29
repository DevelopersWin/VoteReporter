using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	// [ApplyAutoValidation]
	public sealed class ConventionImplementedInterfaces : TransformerBase<Type>
	{
		readonly static ISpecification<Type> Specification = IsPublicTypeSpecification.Default.And( CanActivateSpecification.Default );

		public static IParameterizedSource<Type, Type> Default { get; } = new ConventionImplementedInterfaces().With( Specification );
		ConventionImplementedInterfaces() : this( typeof(ISource), typeof(IParameterizedSource), typeof(IValidatedParameterizedSource) ) {}

		readonly ImmutableArray<Type> exempt;

		public ConventionImplementedInterfaces( params Type[] exempt )
		{
			this.exempt = exempt.ToImmutableArray();
		}

		public override Type Get( Type parameter )
		{
			foreach ( var @interface in parameter.GetTypeInfo().ImplementedInterfaces.Except( exempt ).ToArray() )
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