using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Testing.Framework.Setup.Location;
using Ploeh.AutoFixture;
using System;

namespace DragonSpark.Testing.Framework
{
	public class AssociatedRegistry : Cache<IFixture, IServiceRepository>
	{
		public static AssociatedRegistry Default { get; } = new AssociatedRegistry();

		AssociatedRegistry() : base( instance => new FixtureRegistry( instance ) ) {}
	}

	public sealed class ServiceRegistration : ICustomization
	{
		readonly Type serviceType;

		public ServiceRegistration( Type serviceType )
		{
			this.serviceType = serviceType;
		}

		public void Customize( IFixture fixture )
		{
			var repository = AssociatedRegistry.Default.Get( fixture );
			var instance = GlobalServiceProvider.GetService<object>( serviceType );
			if ( instance.IsAssigned() )
			{
				repository.Add( new InstanceRegistrationRequest( serviceType, instance ) );
			}
		}
	}
}