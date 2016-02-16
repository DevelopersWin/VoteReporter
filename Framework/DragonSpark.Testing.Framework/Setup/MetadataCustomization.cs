using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using Ploeh.AutoFixture;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCustomization : AutoDataCustomization
	{
		public static MetadataCustomization Instance { get; } = new MetadataCustomization();

		readonly Func<MethodBase, ICustomization[]> factory;

		public MetadataCustomization() : this( MetadataCustomizationFactory.Instance.Create )
		{}

		public MetadataCustomization( Func<MethodBase, ICustomization[]> factory )
		{
			this.factory = factory;
		}

		protected override void OnInitializing( AutoData context ) => factory( context.Method ).Each( customization => customization.Customize( context.Fixture ) );
	}

	/*public class UnityContainerFactory<TAssemblyProvider> : UnityContainerFactory<TAssemblyProvider, RecordingMessageLogger> where TAssemblyProvider : IAssemblyProvider
	{
		public new static UnityContainerFactory<TAssemblyProvider> Instance { get; } = new UnityContainerFactory<TAssemblyProvider>();

		protected override IUnityContainer CreateItem() => base.CreateItem().Extension<FixtureExtension>().Container;
	}*/

	public class AutoDataApplication<TSetup> : Application<AutoData> where TSetup : ISetup
	{
		public AutoDataApplication( params ICommand<AutoData>[] commands ) : base( commands.Concat( new SetupApplicationCommand<TSetup>().ToItem() ).ToArray() ) {}
	}

	public abstract class ApplicationSetupCustomization<TApplication, TSetup> : SetupCustomization 
		where TApplication : AutoDataApplication<TSetup>
		where TSetup : class, ISetup
	{
		protected ApplicationSetupCustomization() : base( ActivateFactory<TApplication>.Instance.Create ) {}
	}

	public abstract class SetupCustomization : AutoDataCustomization
	{
		readonly Func<ICommand<AutoData>> setupFactory;
	
		protected SetupCustomization( Func<ICommand<AutoData>> setupFactory )
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