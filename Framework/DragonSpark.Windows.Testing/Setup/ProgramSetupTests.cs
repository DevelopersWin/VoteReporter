using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Composition;
using System.Diagnostics;
using System.Reflection;
using JetBrains.dotMemoryUnit.Util;
using Xunit;

namespace DragonSpark.Windows.Testing.Setup
{
	[Trait( Traits.Category, Traits.Categories.IoC ), ContainingTypeAndNested, FrameworkTypes, AdditionalTypes( typeof(ProgramSetup), typeof(Program) )]
	public class ProgramSetupTests
	{
		/*[Export]
		public IUnityContainer Container { get; } = UnityContainerFactory.Instance.Create();*/

		/*[Theory, AutoData]
		public void Temp()
		{
			Parallel.For( 0, 1000, i =>
								   {
									   var type = ConventionTypes.Instance.Get( typeof(IProgram) );
									   Assert.Equal( typeof(Program), type );
								   } );
		}*/

		/*[Theory, AutoData]
		public void TypeCheck( IUnityContainer container )
		{
			var constructor = container.Resolve<Constructor>().To<IValidatedParameterizedSource>();
			var cancan = constructor.IsSatisfiedBy( typeof(MonitoredModule) );
			Assert.True( cancan );

			var activator = container.Resolve<IActivator>();
			var can = activator.IsSatisfiedBy( typeof(MonitoredModule) );
			Assert.True( can );

			/*var created = activator.Create( typeof(MonitoredModule) );
			Assert.Exists( created );#1#

			/*var activator = sut.Resolve<IActivator>()
			var specification = new DecoratedSpecification<TypeRequest>( sut.Resolve<ResolvableConstructorSpecification>(), ConstructorBase.Coercer.Instance ).To<ISpecification>();
			var valid = specification.IsSatisfiedBy( typeof(MonitoredModule) );
			Assert.True( valid );#1#
		}*/

		/*[Theory, AutoData, IncludeParameterTypes( typeof(ModuleInitializer), typeof(AssemblyModuleCatalog), typeof(ModuleManager), typeof(ModuleMonitor), typeof(MonitoredModule), typeof(TaskMonitor), typeof(MonitoredModule.Command) )]
		public void Extension( IModuleMonitor sut )
		{
			var collection = ListCache.Default.Get( sut );
			var module = collection.FirstOrDefaultOfType<MonitoredModule>();
			Assert.NotNull( module );
			Assert.True( module.Initialized );
			Assert.True( module.Loaded );

			var command = collection.FirstOrDefaultOfType<MonitoredModule.Command>();
			Assert.NotNull( command );
		}*/

		[Theory, AutoData, AdditionalTypes( typeof(AssemblyInformationFactory), typeof(Windows.Runtime.ApplicationAssembly) )]
		public void Create( [EnsureValues, Service]ApplicationInformation sut, [Service]AssemblyInformation temp )
		{
			Assert.NotNull( sut.AssemblyInformation );
			Assert.Equal( DateTimeOffset.Parse( "2/1/2016" ), sut.DeploymentDate.GetValueOrDefault() );
			Assert.Equal( "http://framework.dragonspark.us/testing", sut.CompanyUri.ToString() );
			var assembly = GetType().Assembly;
			Assert.Equal( assembly.From<AssemblyTitleAttribute, string>( attribute => attribute.Title ), sut.AssemblyInformation.Title );
			Assert.Equal( assembly.From<AssemblyCompanyAttribute, string>( attribute => attribute.Company ), sut.AssemblyInformation.Company );
			Assert.Equal( assembly.From<AssemblyCopyrightAttribute, string>( attribute => attribute.Copyright ), sut.AssemblyInformation.Copyright );
			Assert.Equal( assembly.From<DebuggableAttribute, string>( attribute => "DEBUG" ), sut.AssemblyInformation.Configuration );
			Assert.Equal( assembly.From<AssemblyDescriptionAttribute, string>( attribute => attribute.Description ), sut.AssemblyInformation.Description );
			Assert.Equal( assembly.From<AssemblyProductAttribute, string>( attribute => attribute.Product ), sut.AssemblyInformation.Product );
			Assert.Equal( assembly.GetName().Version, sut.AssemblyInformation.Version );
		}

		/*[Theory, AutoData]
		public void Type( IUnityContainer sut )
		{
			var resolve = sut.Resolve<ITyper>();
			Assert.IsType<SomeTypeist>( resolve );
		}*/

		[Theory, AutoData]
		public void Run( [Service]Program sut )
		{
			Assert.True( sut.Ran, "Didn't Run" );
			Assert.Equal( GetType().GetMethod( nameof(Run) ), sut.Arguments.Method );
		}

		/*[Theory, AutoData]
		public void SetupModuleCommand( SetupModuleCommand sut, MonitoredModule module )
		{
			var added = ListCache.Default.Get( module ).FirstOrDefaultOfType<SomeCommand>();
			Assert.Null( added );
			sut.Execute( module );

			Assert.NotNull( ListCache.Default.Get( module ).FirstOrDefaultOfType<SomeCommand>() );
		}*/
	}

	[Shared]
	public class Program : Program<AutoData>
	{
		public bool Ran { get; private set; }

		public AutoData Arguments { get; private set; }

		protected override void Run( AutoData arguments )
		{
			Ran = true;
			Arguments = arguments;
		}
	}

	

	/*public class SomeCommand : ModuleCommand
	{
		public override void Execute( IMonitoredModule parameter ) => ListCache.Default.Get( parameter ).Add( this );
	}

	public class MonitoredModule : MonitoredModule<MonitoredModule.Command>
	{
		public MonitoredModule( IModuleMonitor moduleMonitor, Command command ) : base( moduleMonitor, command )
		{
			ListCache.Default.Get( moduleMonitor ).Add( this );
		}

		public bool Initialized { get; private set; }

		public bool Loaded { get; private set; }
		
		protected override void OnInitialize()
		{
			Initialized = true;
			base.OnInitialize();
		}

		protected override void OnLoad()
		{
			Loaded = true;
			base.OnLoad();
		}

		public class Command : ModuleCommand
		{
			readonly IModuleMonitor monitor;

			public Command( IModuleMonitor monitor )
			{
				this.monitor = monitor;
			}

			public override void Execute( IMonitoredModule parameter ) => ListCache.Default.Get( monitor ).Add( this );
		}
	}*/
}