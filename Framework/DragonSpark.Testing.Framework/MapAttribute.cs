using DragonSpark.Setup.Registration;
using System;

namespace DragonSpark.Testing.Framework
{
	public class MapAttribute : RegistrationBaseAttribute
	{
		public MapAttribute( Type registrationType, Type mappedTo ) : base( t => new RegistrationCustomization( new MappingRegistration( registrationType, mappedTo ) ) ){}

		public class MappingRegistration : TypeRegistration
		{
			public MappingRegistration( Type registrationType, Type mappedTo ) : base( registrationType, mappedTo ) { }
		}
	}
}