using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.Windows.Runtime;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCustomization : AutoDataCustomization
	{
		public static MetadataCustomization Instance { get; } = new MetadataCustomization();

		readonly Func<MethodBase, ICustomization[]> factory;

		public MetadataCustomization() : this( MetadataCustomizationFactory.Instance.Create ) {}

		public MetadataCustomization( Func<MethodBase, ICustomization[]> factory )
		{
			this.factory = factory;
		}

		protected override void OnInitializing( AutoData context ) => factory( context.Method ).Each( customization => customization.Customize( context.Fixture ) );
	}

	public abstract class Application<T> : ApplicationBase where T : ICommand
	{
		protected Application( IServiceProvider context, IEnumerable<ICommand> commands ) : base( context, commands.Append( new ApplyExportedCommandsCommand<T>() ) ) {}
	}

	public interface IApplication : DragonSpark.Setup.IApplication, ICommand<AutoData> { }

	public class ApplicationServiceProviderFactory : DragonSpark.Setup.ApplicationServiceProviderFactory
	{
		public static ApplicationServiceProviderFactory Instance { get; } = new ApplicationServiceProviderFactory();

		public ApplicationServiceProviderFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		AssemblyProvider() : base( new[] { typeof(AssemblySourceBase) }, DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}

	public class Application : ApplicationBase
	{
		public Application() : base( ApplicationServiceProviderFactory.Instance.Create(), Default<ICommand>.Items ) {}
	}

	public abstract class ApplicationBase : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		protected ApplicationBase( IServiceProvider context, IEnumerable<ICommand> commands ) : base( context, commands )
		{
			DisposeAfterExecution = false;
		}
	}
}