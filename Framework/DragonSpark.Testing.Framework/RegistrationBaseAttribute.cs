using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
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

		public void Customize( IFixture fixture ) => registration.Register( new AssociatedRegistry( fixture ).Item );

		public class AssociatedRegistry : AssociatedValue<IFixture, IServiceRegistry>
		{
			public AssociatedRegistry( [Required]IFixture instance ) : base( instance, () => new FixtureRegistry( instance ) ) {}

			/*public AssociatedRegistry( IFixture instance, Func<IServiceRegistry> create = null ) : base( instance, create ) {}*/
		}
	}

	public class RegisterFactoryAttribute : RegistrationBaseAttribute
	{
		public RegisterFactoryAttribute( [Required, OfFactoryType]Type factoryType ) : base( t => new RegistrationCustomization( new FactoryRegistration( factoryType ) ) ) {}
	}

	public class RegisterServiceAttribute : RegistrationBaseAttribute
	{
		public RegisterServiceAttribute( [Required] Type serviceType ) : base( t => new RegistrationCustomization( new ServiceRegistration( serviceType ) ) ) {}
	}

	public class ServiceRegistration : IRegistration, ICustomization
	{
		readonly Type serviceType;

		public ServiceRegistration( [Required] Type serviceType )
		{
			this.serviceType = serviceType;
		}

		public void Register( IServiceRegistry registry )
		{
			Services.Get( serviceType ).With( instance =>
			{
				var parameter = new InstanceRegistrationParameter( serviceType, instance );
				registry.Register( parameter );
			} );
		}

		public void Customize( IFixture fixture ) => Register( new FixtureRegistry( fixture ) );
	}
}