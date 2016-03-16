using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Setup.Registration
{
	public static class Register
	{
		public sealed class TypeAttribute : RegistrationBaseAttribute
		{
			public TypeAttribute( string name = null ) : base( t => new TypeRegistration( t, name ) ) { }
		}

		public sealed class MappedAttribute : RegistrationBaseAttribute
		{
			public MappedAttribute() : this( Services.Get<ImplementedInterfaceFromConventionLocator>, null, null ) {}

			public MappedAttribute( Type @as ) : this( Services.Get<ImplementedInterfaceFromConventionLocator>, @as, null ) { }

			public MappedAttribute( string name ) : this( Services.Get<ImplementedInterfaceFromConventionLocator>, null, name ) { }

			MappedAttribute( [Required]Func<ImplementedInterfaceFromConventionLocator> locator, Type @as, string name ) : base( t => new TypeRegistration( @as ?? locator().Create( t ) ?? t, t, name ) ) { }
		}

		public class FactoryAttribute : RegistrationBaseAttribute
		{
			public FactoryAttribute() : this( Services.Get<ISingletonLocator> ) {
			}
			public FactoryAttribute( Func<ISingletonLocator> locator ) : base( t => new FactoryRegistration( locator(), t ) ) { }
		}
	}
}