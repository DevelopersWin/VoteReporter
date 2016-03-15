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

	public abstract class Application<T> : Application where T : ICommand
	{
		protected Application( IApplicationContext context, IEnumerable<ICommand> commands ) : base( context, commands.Append( new ApplyExportedCommandsCommand<T>() ) ) {}
	}

	public interface IApplication : DragonSpark.Setup.IApplication, ICommand<AutoData> { }

	public class ApplicationContextFactory : DragonSpark.Setup.ApplicationContextFactory
	{
		public static ApplicationContextFactory Instance { get; } = new ApplicationContextFactory();

		public ApplicationContextFactory() : base( () => Default<Assembly>.Items, CompositionHostFactory.Instance.Create, DefaultServiceLocatorFactory.Instance.Create ) {}
	}

	public class DefaultApplication : Application
	{
		public DefaultApplication() : base( ApplicationContextFactory.Instance.Create(), Default<ICommand>.Items ) {}
	}

	public abstract class Application : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		protected Application( IApplicationContext context, IEnumerable<ICommand> commands ) : base( context, commands )
		{
			DisposeAfterExecution = false;
		}
	}
}