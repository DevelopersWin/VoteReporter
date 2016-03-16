using System;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Testing.Framework.Setup.Location
{
	class FixtureRegistry : IServiceRegistry
	{
		readonly IFixture fixture;

		public FixtureRegistry( [Required]IFixture fixture )
		{
			this.fixture = fixture;
		}

		public void Register( [Required]MappingRegistrationParameter parameter ) => fixture.Customizations.Add( new TypeRelay( parameter.Type, parameter.MappedTo ) );

		public void Register( [Required]InstanceRegistrationParameter parameter ) => this.InvokeGenericAction( nameof(RegisterInstance), new[] { parameter.Type }, parameter.Instance );

		void RegisterInstance<T>( [Required]T instance ) => fixture.Inject( instance );

		public void RegisterFactory( [Required]FactoryRegistrationParameter parameter ) => this.InvokeGenericAction( nameof(RegisterFactory), parameter.Type.ToItem(), parameter.Factory );

		void RegisterFactory<T>( [Required]Func<object> factory ) => fixture.Customize<T>( c => c.FromFactory( () => (T)factory() ).OmitAutoProperties() );
	}
}