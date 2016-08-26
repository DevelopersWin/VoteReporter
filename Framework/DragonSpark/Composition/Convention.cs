using DragonSpark.Application;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using System.Linq;
using Defaults = DragonSpark.Sources.Parameterized.Defaults;

namespace DragonSpark.Composition
{
	[ApplyAutoValidation]
	public sealed class ConventionTypes : ValidatedParameterizedSourceBase<Type, Type>
	{
		readonly static ISpecification<Type> Specification = InstantiableTypeSpecification.Default.And( CanInstantiateSpecification.Default.Inverse() );
		readonly static Func<Type, bool> Activate = Defaults.ActivateSpecification.IsSatisfiedBy;

		public static IParameterizedSource<Type, Type> Default { get; } = new ParameterizedScope<Type, Type>( new ConventionTypes().ToSourceDelegate().Global() );
		ConventionTypes() : this( ApplicationTypes.Default ) {}

		readonly ISource<ImmutableArray<Type>> source;

		public ConventionTypes( ISource<ImmutableArray<Type>> source ) : base( Specification )
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
			var adapter = parameter.Adapt();
			var convention = IsConventionCandidateSpecification.Defaults.Get( parameter );
			var result =
					source.Get()
					.Where( adapter.IsAssignableFrom )
					.Where( Activate )
					.FirstOrDefault( convention );
			return result;
		}

		public override Type Get( Type parameter ) => Map( parameter ) ?? Search( parameter );
	}
}
