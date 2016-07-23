using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup.Registration;
using DragonSpark.Testing.Framework.Setup.Location;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System;
using Type = System.Type;

namespace DragonSpark.Testing.Framework
{
	[AttributeUsage( AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true )]
	public abstract class RegistrationBaseAttribute : HostingAttribute
	{
		protected RegistrationBaseAttribute( Func<object, ICustomization> factory ) : base( x => x.AsTo( factory ) ) {}
	}

	public class RegistrationCustomization : ICustomization
	{
		readonly IRegistration registration;

		public RegistrationCustomization( [Required]IRegistration registration )
		{
			this.registration = registration;
		}

		public void Customize( IFixture fixture ) => registration.Register( AssociatedRegistry.Default.Get( fixture ) );
	}

	public class AssociatedRegistry : Cache<IFixture, IServiceRegistry>
	{
		public static AssociatedRegistry Default { get; } = new AssociatedRegistry();

		AssociatedRegistry() : base( instance => new FixtureRegistry( instance ) ) {}
	}

	public class RegisterFactoryAttribute : RegistrationBaseAttribute
	{
		public RegisterFactoryAttribute( [Required, OfFactoryType]Type factoryType ) : base( t => new RegistrationCustomization( new FactoryRegistration( factoryType ) ) ) {}
	}

	/*public class RegisterServiceAttribute : RegistrationBaseAttribute
	{
		public RegisterServiceAttribute( [Required] Type serviceType ) : base( t => new RegistrationCustomization( new ServiceRegistration( serviceType ) ) ) {}
	}*/

	public class ServiceRegistration : IRegistration, ICustomization
	{
		readonly Type serviceType;

		public ServiceRegistration( [Required] Type serviceType )
		{
			this.serviceType = serviceType;
		}

		public void Register( IServiceRegistry registry )
		{
			var instance = GlobalServiceProvider.Instance.Get<object>( serviceType );
			if ( instance.IsAssigned() )
			{
				var parameter = new InstanceRegistrationParameter( serviceType, instance );
				registry.Register( parameter );
			}
		}

		public void Customize( IFixture fixture ) => Register( AssociatedRegistry.Default.Get( fixture ) );
	}
}