using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCommand : AutoDataCommandBase
	{
		public static MetadataCommand Instance { get; } = new MetadataCommand();

		readonly Func<MethodBase, ICustomization[]> factory;

		public MetadataCommand() : this( MetadataCustomizationFactory.Instance.Create ) {}

		public MetadataCommand( Func<MethodBase, ICustomization[]> factory )
		{
			this.factory = factory;
		}

		protected override void OnExecute( AutoData parameter )
		{
			var customizations = factory( parameter.Method );
			customizations.Each( customization => customization.Customize( parameter.Fixture ) );
		}
	}

	public class Application<T> : Application where T : ICommand
	{
		public Application( IServiceProvider provider ) : this( provider, Default<ICommand>.Items ) {}

		public Application( IServiceProvider provider, IEnumerable<ICommand> commands ) : base( provider, commands.Append( new ApplyExportedCommandsCommand<T>() ) ) {}
	}

	public interface IApplication : IApplication<AutoData> { }

	/*public class Application : ApplicationBase
	{
		// public Application() : this( ServiceProviderFactory.Instance.Create() ) {}

		public Application( IServiceProvider provider ) : base( provider ) {}
		
	}*/

	public class Application : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		public Application( IServiceProvider provider ) : this( provider, Default<ICommand>.Items ) {}

		public Application( IServiceProvider provider, IEnumerable<ICommand> commands ) : base( provider, MetadataCommand.Instance.Append( commands ) ) {}

		/*protected override void OnExecute( AutoData parameter )
		{
			/*var registry = Services.Get<IExportDescriptorProviderRegistry>();
			registry.Register( new InstanceExportDescriptorProvider<AutoData>( parameter ) );#1#
			
			base.OnExecute( parameter );
		}*/

		/*public override object GetService( Type serviceType )
		{
			var result = new[]
			{
				Services.Get<AutoData>().With( data => new AssociatedFactory( data.Fixture ).Item ),
				base.GetService
			}.NotNull().FirstWhere( func => func( serviceType ) );
			return result;
		}*/
	}
}