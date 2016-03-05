using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using System;
using System.Linq;
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

	public class Application : Application<AutoData>
	{
		public Application( params ICommand<AutoData>[] commands ) : base( commands ) {}

		// public Application( params ICommand<AutoData>[] commands ) : base( commands.Concat( new SetupApplicationCommand<TSetup>().ToItem() ).ToArray() ) {}
	}

	/*public abstract class ApplicationSetupCustomization<TApplication> : SetupCustomization 
		where TApplication : Application
	{
		protected ApplicationSetupCustomization() : base( ActivateFactory<TApplication>.Instance.Create ) {}
	}*/

	public class ApplicationCustomization<T> : ApplicationCustomization where T : ICommand
	{
		public ApplicationCustomization() : this( Default<ICommand>.Items ) {}

		public ApplicationCustomization( params ICommand[] commands ) : base( () => new Application( commands.Concat( new [] { new ApplyExportedCommandsCommand<T>() } ).Cast<ICommand<object>>().ToArray() ) ) {}
	}

	public class ApplicationCustomization : AutoDataCustomization
	{
		readonly Func<ICommand<AutoData>> setupFactory;
	
		protected ApplicationCustomization( Func<ICommand<AutoData>> setupFactory )
		{
			this.setupFactory = setupFactory;
		}

		protected override void OnInitializing( AutoData context )
		{
			var setup = setupFactory();
			setup.Run( context );
		}
	}
}