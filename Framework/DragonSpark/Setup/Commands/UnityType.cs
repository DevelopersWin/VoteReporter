using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using System;

namespace DragonSpark.Setup.Commands
{
	public class UnityType : UnityRegistrationCommand
	{
		public Type MapTo { get; set; }
		
		public override void Execute( object parameter )
		{
			var registry = new ServiceRegistry( Container, Lifetime );
			registry.Register( new MappingRegistrationParameter( RegistrationType, MapTo ?? RegistrationType, BuildName ) );
		}
	}
}