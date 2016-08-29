using DragonSpark.Activation;
using DragonSpark.Application;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Composition
{
	[ApplyAutoValidation]
	public sealed class ConventionTypes : ValidatedParameterizedSourceBase<Type, Type>
	{
		readonly static ISpecification<Type> Specification = CanActivateSpecification.Default.Inverse();
		// readonly static Func<Type, bool> Instantiable = Activation.Defaults.Instantiable.IsSatisfiedBy;

		public static IParameterizedSource<Type, Type> Default { get; } = new ParameterizedScope<Type, Type>( new ConventionTypes().ToSourceDelegate().Fix().Global() );
		ConventionTypes() : this( ApplicationTypes.Default.ToDelegate() ) {}

		readonly Func<ImmutableArray<Type>> source;

		public ConventionTypes( Func<ImmutableArray<Type>> source ) : base( Specification )
		{
			this.source = source;
		}

		static Type Map( Type parameter )
		{
			var name = $"{parameter.Namespace}.{ConventionCandidateNames.Default.Get( parameter )}";
			var result = name != parameter.FullName ? parameter.Assembly().GetType( name ) : null;
			return result;
		}

		Type Search( Type parameter )
		{
			// var adapter = parameter.Adapt();
			var convention = IsConventionCandidateSpecification.Defaults.Get( parameter );
			var result =
					source()
					/*.Where( adapter.IsAssignableFrom )
					.Where( Instantiable )*/
					.FirstOrDefault( convention );
			return result;
		}

		public override Type Get( Type parameter ) => Map( parameter ) ?? Search( parameter );
	}
}