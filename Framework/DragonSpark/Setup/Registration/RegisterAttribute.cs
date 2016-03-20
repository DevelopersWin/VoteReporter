using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;
using DragonSpark.Extensions;

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
			public MappedAttribute() : this( ImplementedInterfaceFromConventionLocator.Instance, null, null ) {}

			public MappedAttribute( Type @as ) : this( ImplementedInterfaceFromConventionLocator.Instance, @as, null ) { }

			public MappedAttribute( string name ) : this( ImplementedInterfaceFromConventionLocator.Instance, null, name ) { }

			MappedAttribute( [Required]ImplementedInterfaceFromConventionLocator locator, Type @as, string name ) : base( t => new TypeRegistration( @as ?? locator.Create( t ) ?? t.GetTypeInfo().ImplementedInterfaces.Only() ?? t, t, name ) ) { }
		}

		public class FactoryAttribute : RegistrationBaseAttribute
		{
			public FactoryAttribute() : this( Services.Get<ISingletonLocator> ) {}

			public FactoryAttribute( [Required]Func<ISingletonLocator> locator ) : base( t => new FactoryRegistration( locator(), t ) ) { }
		}
	}
}