using DragonSpark.Aspects.Validation;
using DragonSpark.Composition;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Composition;

namespace DragonSpark.Activation.Location
{
	public sealed class Singletons : DelegatedParameterizedSource<Type, object>, ISingletonLocator
	{
		public static ISpecification<Type> Specification { get; } = Source.DefaultSpecification.And( ContainsSingletonPropertySpecification.Default );

		[Export]
		public static ISingletonLocator Default { get; } = new Singletons();
		Singletons() : base( Source.DefaultNested.ToSourceDelegate() ) {}

		public Singletons( Func<Type, object> inner ) : base( inner ) {}

		[ApplyAutoValidation]
		sealed class Source : ValidatedParameterizedSourceBase<Type, object>
		{
			readonly static Transform<Type> Convention = ConventionTypes.Default.Get;
			
			public static IParameterizedSource<Type, object> DefaultNested { get; } = new Source().ToCache();
			Source() : this( Convention, SingletonDelegates.Default.Get ) {}

			readonly Transform<Type> convention;
			readonly Func<Type, Func<object>> delegateSource;

			Source( Transform<Type> convention, Func<Type, Func<object>> delegateSource ) : base( Specification )
			{
				this.convention = convention;
				this.delegateSource = delegateSource;
			}

			public override bool IsSatisfiedBy( Type parameter ) => base.IsSatisfiedBy( convention( parameter ) ?? parameter );

			public override object Get( Type parameter ) => delegateSource( parameter )?.Invoke();
		}
	}
}