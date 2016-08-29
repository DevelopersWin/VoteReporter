using DragonSpark.Aspects.Validation;
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
	[ApplyAutoValidation]
	public sealed class ConventionImplementedInterfaces : ValidatedParameterizedSourceBase<Type, Type>
	{
		public static ConventionImplementedInterfaces Default { get; } = new ConventionImplementedInterfaces();
		ConventionImplementedInterfaces() : this( typeof(ISource), typeof(IParameterizedSource), typeof(IValidatedParameterizedSource) ) {}

		readonly ImmutableArray<Type> exempt;

		public ConventionImplementedInterfaces( params Type[] exempt ) : base( DefaultSpecification.And( CanActivateSpecification.Default, new DelegatedSpecification<Type>( type =>
																																											 {
																																												 var result = type.GetTypeInfo().IsPublic || type.GetTypeInfo().IsNestedPublic;
																																												 return result;
																																											 } ) ) )
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