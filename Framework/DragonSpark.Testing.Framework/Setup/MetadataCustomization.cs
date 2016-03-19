using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Input;
using Type = System.Type;

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
		protected Application( AutoData autoData, IServiceProvider context, IEnumerable<ICommand> commands ) : base( autoData, context, commands.Append( new ApplyExportedCommandsCommand<T>() ) ) {}
	}

	public interface IApplication : DragonSpark.Setup.IApplication, ICommand<AutoData> { }

	public class ServiceProviderFactory : DragonSpark.Setup.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		AssemblyProvider() : base( new[] { typeof(AssemblySourceBase) }, DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}

	public class Application : ApplicationBase
	{
		public Application( AutoData autoData ) : this( autoData, ServiceProviderFactory.Instance.Create() ) {}

		public Application( AutoData autoData, IServiceProvider provider ) : base( autoData, provider ) {}
	}

	public abstract class ApplicationBase : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		readonly AutoData autoData;

		protected ApplicationBase( AutoData autoData, IServiceProvider context ) : this( autoData, context, Default<ICommand>.Items ) {}

		protected ApplicationBase( [Required]AutoData autoData, IServiceProvider context, IEnumerable<ICommand> commands ) : base( context, commands )
		{
			this.autoData = autoData;
			DisposeAfterExecution = false;
		}

		public override object GetService( Type serviceType ) => base.GetService( serviceType ) ?? FromAutoData( serviceType );

		object FromAutoData( Type serviceType )
		{
			var registered = new RegistrationCustomization.AssociatedRegistry( autoData.Fixture ).Item.IsRegistered( serviceType );
			var result = registered ? autoData.Fixture.Create<object>( serviceType ) : null;
			return result;
		}
	}
}