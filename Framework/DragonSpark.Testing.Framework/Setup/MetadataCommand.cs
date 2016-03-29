using DragonSpark.Extensions;
using DragonSpark.Runtime;
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
	public class MetadataCommand : AutoDataCommand
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

	public abstract class Application<T> : ApplicationBase where T : ICommand
	{
		protected Application( IServiceProvider context, IEnumerable<ICommand> commands ) : base( context, commands.Append( new ApplyExportedCommandsCommand<T>() ) ) {}
	}

	public interface IApplication : IApplication<AutoData> { }

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		AssemblyProvider() : base( new[] { typeof(AssemblySourceBase) }, DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}

	public class Application : ApplicationBase
	{
		// public Application() : this( ServiceProviderFactory.Instance.Create() ) {}

		public Application( IServiceProvider context ) : base( context ) {}
		
	}

	public abstract class ApplicationBase : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		protected ApplicationBase( IServiceProvider context ) : this( context, Default<ICommand>.Items ) {}

		protected ApplicationBase( IServiceProvider context, IEnumerable<ICommand> commands ) : base( context, MetadataCommand.Instance.Append( commands ) ) {}

		protected override void OnExecute( AutoData parameter )
		{
			/*var registry = Services.Get<IExportDescriptorProviderRegistry>();
			registry.Register( new InstanceExportDescriptorProvider<AutoData>( parameter ) );*/
			
			base.OnExecute( parameter );
		}

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