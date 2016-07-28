using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using System;
using System.Reflection;

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
			public MappedAttribute() : this( null, null ) {}

			public MappedAttribute( Type @as ) : this( @as, null ) { }

			public MappedAttribute( string name ) : this( null, name ) { }

			MappedAttribute( Type @as, string name ) : base( t => new TypeRegistration( @as ?? ConventionImplementedInterfaces.Instance.Get( t ) ?? t.GetTypeInfo().ImplementedInterfaces.Only() ?? t, t, name ) ) { }
		}

		/*public class FactoryAttribute : RegistrationBaseAttribute
		{
			public FactoryAttribute() : base( t => new FactoryRegistration( t ) ) { }
		}*/
	}
}