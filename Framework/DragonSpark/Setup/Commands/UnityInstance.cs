using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Windows.Markup;

namespace DragonSpark.Setup.Commands
{
	[ContentProperty( nameof(Instance) )]
	public class UnityInstance : UnityRegistrationCommand
	{
		[Required]
		public object Instance { [return: Required]get; set; }

		public override void Execute( object parameter )
		{
			var type = RegistrationType ?? Instance.With( item => item.GetType() );
			var registration = Instance.ConvertTo( type );

			var registry = new ServiceRegistry( Container, Lifetime );
			registry.Register( new InstanceRegistrationParameter( type, registration, BuildName ) );
		}
	}
}