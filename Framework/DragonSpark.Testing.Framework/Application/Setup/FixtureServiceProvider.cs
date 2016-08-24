using DragonSpark.Application.Setup;
using DragonSpark.Aspects.Validation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.Testing.Framework.Runtime;
using Ploeh.AutoFixture;
using System;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	[ApplyAutoValidation]
	sealed class FixtureServiceProvider : ValidatedParameterizedSourceBase<Type, object>, IServiceProvider
	{
		readonly IFixture fixture;

		public FixtureServiceProvider( IFixture fixture ) : base( new Specification( fixture ) )
		{
			this.fixture = fixture;
		}

		public override object Get( Type parameter ) => fixture.Create<object>( parameter );

		public object GetService( Type serviceType ) => Get( serviceType );

		sealed class Specification : SpecificationBase<Type>
		{
			readonly IServiceRepository registry;

			public Specification( IFixture fixture ) : this( AssociatedRegistry.Default.Get( fixture ) ) {}

			Specification( IServiceRepository registry )
			{
				this.registry = registry;
			}

			public override bool IsSatisfiedBy( Type parameter ) => registry.IsSatisfiedBy( parameter );
		}
	}
}