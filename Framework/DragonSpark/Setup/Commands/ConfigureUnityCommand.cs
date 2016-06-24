using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Microsoft.Practices.Unity;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Setup.Commands
{
	[ContentProperty( nameof(Instances) )]
	public class ConfigureUnityCommand : UnityCommand
	{
		public System.Collections.ObjectModel.Collection<UnityContainerExtension> Extensions { get; } = new System.Collections.ObjectModel.Collection<UnityContainerExtension>();

		public CommandCollection<UnityType> Types { get; } = new CommandCollection<UnityType>();

		public UnityInstanceCollection Instances { get; } = new UnityInstanceCollection();

		public override void Execute( object parameter )
		{
			Extensions.Each( Container.AddExtension );

			foreach ( var source in Instances.Cast<UnityCommand>().Concat( Types ).ToArray() )
			{
				source.Execute( parameter );
			}
		}
	}

	public class UnityInstanceCollection : CommandCollection<UnityInstance>
	{
		protected override UnityInstance OnAdd( object item ) => base.OnAdd( item ) ?? new UnityInstance { Instance = item, RegistrationType = item.GetType() };
	}
}
