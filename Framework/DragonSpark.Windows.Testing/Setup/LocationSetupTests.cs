using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Setup.Registration;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Testing.TestObjects;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using PostSharp.Patterns.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Activator = DragonSpark.Windows.Testing.TestObjects.Activator;
using Attribute = DragonSpark.Testing.Objects.Attribute;
using Object = DragonSpark.Testing.Objects.Object;
using ServiceLocator = DragonSpark.Activation.IoC.ServiceLocator;

namespace DragonSpark.Windows.Testing.Setup
{
	/// <summary>
	/// This file can be seen as a bucket for all the testing done around setup.  It also can be seen as a huge learning bucket for xUnit and AutoFixture.  This does not contain best practices.  Always be learning. :)
	/// </summary>
	public class LocationSetupTests : Tests
	{
		public LocationSetupTests( ITestOutputHelper output ) : base( output )
		{}

		[Theory, LocationSetup.AutoData]
		public void CoreLocation( IServiceLocator sut )
		{
			Assert.True( Microsoft.Practices.ServiceLocation.ServiceLocator.IsLocationProviderSet );
			Assert.Same( sut, Microsoft.Practices.ServiceLocation.ServiceLocator.Current );
		}

		[Theory, LocationSetup.AutoData]
		public void MockAsExpected( [Located(false)]ISetup sut )
		{
			Assert.NotNull( Mock.Get( sut ) );
		}

		[Theory, LocationSetup.AutoData]
		public void SetupRegistered( ISetup sut )
		{
			Assert.IsType<LocationSetup>( sut );
		}

		[Map( typeof(IActivator), typeof(Activator))]
		[Theory, LocationSetup.AutoData]
		public void CreateInstance( [Registered]IActivator activator )
		{
			var expected = Services.Get<IActivator>();
			Assert.Same( expected, activator );
			Assert.NotSame( SystemActivator.Instance, activator );
			Assert.IsType<Activator>( activator );
			var instance = activator.Activate<IObject>( typeof(Object) );
			Assert.IsType<Object>( instance );

			Assert.Equal( "DefaultActivation", instance.Name );
		}

		[Map( typeof( IActivator ), typeof( Activator ) )]
		[Theory, LocationSetup.AutoData]
		public void CreateNamedInstance( [Registered]IActivator activator, string name )
		{
			Assert.Same( Services.Get<IActivator>(), activator );
			Assert.NotSame( SystemActivator.Instance, activator );

			var instance = activator.Activate<IObject>( typeof(Object), name );
			Assert.IsType<Object>( instance );

			Assert.Equal( name, instance.Name );
		}

		[Map( typeof(IActivator), typeof(Activator) )]
		[Theory, LocationSetup.AutoData]
		public void CreateItem( [Registered]IActivator activator )
		{
			var parameters = new object[] { typeof(Object), "This is Some Name." };
			Assert.Same( Services.Get<IActivator>(), activator );
			Assert.NotSame( SystemActivator.Instance, activator );
			var instance = activator.Construct<DragonSpark.Testing.Objects.Item>( parameters );
			Assert.NotNull( instance );

			Assert.Equal( parameters, instance.Parameters );
		}

		[Theory, LocationSetup.AutoData]
		void RegisterInstanceGeneric( [Located]IServiceRegistry registry, Class instance )
		{
			registry.Register<IInterface>( instance );

			var located = Services.Get<IInterface>();
			Assert.IsType<Class>( located );
			Assert.Equal( instance, located );
		}

		[Theory, LocationSetup.AutoData]
		public void RegisterGeneric()
		{
			var registry = Services.Get<IServiceRegistry>();
			registry.Register<IInterface, Class>();

			var located = Services.Get<IInterface>();
			Assert.IsType<Class>( located );
		}

		[Theory, LocationSetup.AutoData]
		public void RegisterLocation()
		{
			var registry = Services.Get<IServiceRegistry>();
			registry.Register<IInterface, Class>();

			var located = Services.Get<IInterface>();
			Assert.IsType<Class>( located );
		}

		[Theory, LocationSetup.AutoData]
		void RegisterInstanceClass( Class instance )
		{
			var registry = Services.Get<IServiceRegistry>();
			registry.Register<IInterface>( instance );

			var located = Services.Get<IInterface>();
			Assert.IsType<Class>( located );
			Assert.Equal( instance, located );
		}

		[Theory, LocationSetup.AutoData]
		void RegisterFactoryClass( Class instance )
		{
			var registry = Services.Get<IServiceRegistry>();
			registry.Register<IInterface>( () => instance );

			var located = Services.Get<IInterface>();
			Assert.IsType<Class>( located );
			Assert.Equal( instance, located );
		}

		[Theory, LocationSetup.AutoData]
		public void With( IServiceLocator locator, [Frozen, Registered]ClassWithParameter instance )
		{
			var item = locator.GetInstance<ClassWithParameter>().With( x => x.Parameter );
			Assert.Equal( instance.Parameter, item );

			Assert.Null( locator.GetInstance<IInterface>().With( x => x ) );
		}

		[Theory, LocationSetup.AutoData]
		public void WithDefault()
		{
			var item = Services.Get<ClassWithParameter>().With( x => x.Parameter != null );
			Assert.True( item );
		}

		[Theory, LocationSetup.AutoData]
		public void RegisterWithRegistry( Mock<IServiceRegistry> sut )
		{
			var registry = Services.Get<IServiceRegistry>();
			registry.Register( sut.Object );

			var registered = Services.Get<IServiceRegistry>();
			Assert.Same( sut.Object, registered );
			registered.Register<IInterface, Class>();

			sut.Verify( x => x.Register( It.Is<MappingRegistrationParameter>( parameter => parameter.MappedTo == typeof(Class) && parameter.Type == typeof(IInterface) && parameter.Name == null ) ) );
		}

		[Theory, LocationSetup.AutoData]
		void Resolve( [Located]Interfaces sut )
		{
			Assert.NotNull( sut.Items.FirstOrDefaultOfType<Item>() );
			Assert.NotNull( sut.Items.FirstOrDefaultOfType<AnotherItem>() );
			Assert.NotNull( sut.Items.FirstOrDefaultOfType<YetAnotherItem>() );
		}

		[Theory, LocationSetup.AutoData]
		void GetInstance( [Factory, Frozen( Matching.ExactType )]ServiceLocator sut, [Frozen, Registered]Mock<ILogger> logger )
		{
			Assert.Same( sut.Get<ILogger>(), logger.Object );

			var before = sut.GetInstance<IInterface>();
			Assert.Null( before );

			var registry = sut.GetInstance<IServiceRegistry>();
			registry.Register<IInterface, Class>();

			var after = sut.GetInstance<IInterface>();
			Assert.IsType<Class>( after );

			var broken = sut.GetInstance<ClassWithBrokenConstructor>();
			Assert.Null( broken );
		}

		[Theory, LocationSetup.AutoData]
		void GetAllInstancesLocator( [Modest, Factory] ServiceLocator sut )
		{
			var registry = sut.GetInstance<IServiceRegistry>();
			registry.Register<IInterface, Class>( "First" );
			registry.Register<IInterface, Derived>( "Second" );

			var count = sut.Container.Registrations.Count( x => x.RegisteredType == typeof(IInterface) );
			Assert.Equal( 2, count );

			var items = sut.GetAllInstances<IInterface>();
			Assert.Equal( 2, items.Count() );

			var classes = new[]{ new Class() };
			registry.Register<IEnumerable<IInterface>>( classes );

			var updated = sut.GetAllInstances<IInterface>().Fixed();
			Assert.Equal( 3, updated.Length );

			Assert.Contains( classes.Single(), updated );
		}

		[Theory, LocationSetup.AutoData]
		void CreateActivator( IActivator sut, string message, int number, Class @item )
		{
			Assert.IsAssignableFrom<CompositeActivator>( sut );

			var created = sut.Construct<ClassWithManyParameters>( number, message, item );
			Assert.Equal( message, created.String );
			Assert.Equal( number, created.Integer );
			Assert.Equal( item, created.Class );

			var systemMessage = "Create from system";
			var systemCreated = sut.Construct<ClassCreatedFromDefault>( systemMessage );
			Assert.NotNull( systemCreated );
			Assert.Equal( systemMessage, systemCreated.Message );
		}

		[Theory, LocationSetup.AutoData]
		void Register( IUnityContainer container, [Greedy, Frozen]ServiceLocator sut )
		{
			var registry = sut.GetInstance<IServiceRegistry>();
			Assert.False( container.IsRegistered<IInterface>() );
			registry.Register<IInterface, Class>();
			Assert.True( container.IsRegistered<IInterface>() );
			Assert.IsType<Class>( container.Resolve<IInterface>() );
		}

		[Theory, LocationSetup.AutoData]
		void RegisterInstance( IUnityContainer container, [Greedy, Frozen]ServiceLocator sut )
		{
			Assert.False( container.IsRegistered<IInterface>() );
			var instance = new Class();
			var registry = sut.GetInstance<IServiceRegistry>();
			registry.Register<IInterface>( instance );
			Assert.True( container.IsRegistered<IInterface>() );
			Assert.IsType<Class>( container.Resolve<IInterface>() );
			Assert.Equal( instance, container.Resolve<IInterface>() );
		}

		[Theory, LocationSetup.AutoData]
		void RegisterFactory( IUnityContainer container, [Greedy, Frozen]ServiceLocator sut )
		{
			Assert.False( container.IsRegistered<IInterface>() );
			var registry = sut.GetInstance<IServiceRegistry>();
			registry.RegisterFactory( new FactoryRegistrationParameter( typeof(IInterface), () => new Class() ) );
			Assert.True( container.IsRegistered<IInterface>() );
			Assert.IsType<Class>( container.Resolve<IInterface>() );
		}

		[Theory, LocationSetup.AutoData]
		void Dispose( [Factory, Frozen] ServiceLocator sut )
		{
			var item = Services.Get<IInterface>( typeof(Class) );
			Assert.NotNull( item );

			var disposable = new Disposable();

			var registry = sut.GetInstance<IServiceRegistry>();
			registry.Register( disposable );
			// registry.Register( new ServiceLocationMonitor( Services.Location, sut ) );

			Assert.False( disposable.Disposed );

			// Assert.Same( Services.Location.Item, sut );

			sut.QueryInterface<IDisposable>().Dispose();

			// Assert.NotSame( Services.Location.Item, sut );

			Assert.True( disposable.Disposed );
		}

		[Theory, LocationSetup.AutoData]
		void RelayedPropertyAttribute()
		{
			var attribute = typeof(Relayed).GetProperty( nameof(Relayed.Property) ).GetAttribute<Attribute>();
			Assert.Equal( "This is a relayed property attribute.", attribute.PropertyName );
		}


		[Theory, LocationSetup.AutoData]
		void RelayedAttribute()
		{
			var attribute = typeof(Relayed).GetAttribute<Attribute>();
			Assert.Equal( "This is a relayed class attribute.", attribute.PropertyName );
		}

		[Theory, LocationSetup.AutoData]
		[Map(typeof(IExceptionFormatter), typeof(ExceptionFormatter) )]
		public void Try()
		{
			var exception = ExceptionSupport.Try( () => {} );
			Assert.Null( exception );
		}

		[Theory, LocationSetup.AutoData]
		public void GetAllTypesWith( [Located]Assembly[] sut )
		{
			var items = sut.GetAllTypesWith<PriorityAttribute>();
			Assert.True( items.Select( tuple => tuple.Item2 ).AsTypes().Contains( typeof(NormalPriority) ) );
		}

		[Theory, LocationSetup.AutoData]
		public void Evaluate( ClassWithParameter sut )
		{
			Assert.Equal( sut.Parameter, sut.Evaluate<object>( nameof(sut.Parameter) ) );
		}

		[Theory, LocationSetup.AutoData]
		public void Mocked( [Frozen]Mock<IInterface> sut, IInterface item )
		{
			Assert.Equal( sut.Object, item );
		}

		[Theory, LocationSetup.AutoData]
		public void GetAllInstances( IServiceLocator sut )
		{
			Assert.False( sut.GetAllInstances<Class>().Any() );
		}

		[Theory, LocationSetup.AutoData]
		public void Singleton( [Located] IUnityContainer sut )
		{
			var once = sut.Resolve<RegisterAsSingleton>();
			var twice = sut.Resolve<RegisterAsSingleton>();
			Assert.Same( once, twice );
		}

		[Theory, LocationSetup.AutoData]
		public void Many( [Located] IUnityContainer sut )
		{
			var once = sut.Resolve<RegisterAsMany>();
			var twice = sut.Resolve<RegisterAsMany>();
			Assert.NotSame( once, twice );
		}

		[AssemblyProvider.Register]
		[Theory, LocationSetup.AutoData]
		public void Factory( AllTypesOfFactory sut )
		{
			var items = sut.Create<IInterface>();
			Assert.True( items.Any() );
			Assert.NotNull( items.FirstOrDefaultOfType<YetAnotherClass>() );
		}

		[Theory, LocationSetup.AutoData]
		public void Locate( [Located]ApplicationAssemblyLocator sut )
		{
			Assert.Same( GetType().Assembly, sut.Create() );
		}

		[Theory, LocationSetup.AutoData]
		public void RegisterWithName( IServiceLocator locator )
		{
			Assert.Null( locator.GetInstance<IRegisteredWithName>() );

			var located = locator.GetInstance<IRegisteredWithName>( "Registered" );
			Assert.IsType<MappedWithNameClass>( located );
		}

		[Theory, LocationSetup.AutoData]
		public void CreateAssemblies( IUnityContainer container, IAssemblyProvider provider, [Located]Assembly[] sut )
		{
			var registered = container.IsRegistered<Assembly[]>();
			Assert.True( registered );

			var fromContainer = container.Resolve<Assembly[]>();
			var fromProvider = provider.Create();
			Assert.Equal( fromContainer, fromProvider );

			Assert.Equal( fromContainer, sut );
		}

		[Theory, LocationSetup.AutoData]
		public void ConventionLocator( BuildableTypeFromConventionLocator locator )
		{
			var type = locator.Create( typeof(Assembly) );
			Assert.Null( type );
		}

		[Theory, LocationSetup.AutoData]
		public void CreateAssemblySimple( IUnityContainer container, IApplicationAssemblyLocator locator, [Located] Assembly sut )
		{
			Assert.True( container.IsRegistered<Assembly>() );
		}

		[Theory, LocationSetup.AutoData]
		public void BasicComposition( Assembly[] assemblies, ExportDescriptorProvider provider )
		{
			using ( var container = new CompositionHostFactory( assemblies ).Create() )
			{
				container.GetExport<IExportDescriptorProviderRegistry>().Register( provider );

				var test = container.GetExport<Assembly>();
				Assert.NotNull( test );
			}
		}

		[Theory, LocationSetup.AutoData]
		public void CreateAssembly( AssemblyInformationFactory factory, IUnityContainer container, IApplicationAssemblyLocator locator, [Located]Assembly sut )
		{
			Assert.True( container.IsRegistered<Assembly>() );

			var fromFactory = locator.Create();
			var fromContainer = container.Resolve<Assembly>();
			Assert.Same( fromFactory, fromContainer );
			

			Assert.Same( fromContainer, sut );

			var fromProvider = factory.Create( fromFactory );
			Assert.NotNull( fromProvider );

			var assembly = GetType().Assembly;
			Assert.Equal( assembly.From<AssemblyTitleAttribute, string>( attribute => attribute.Title ), fromProvider.Title );
			Assert.Equal( assembly.From<AssemblyCompanyAttribute, string>( attribute => attribute.Company ), fromProvider.Company );
			Assert.Equal( assembly.From<AssemblyCopyrightAttribute, string>( attribute => attribute.Copyright ), fromProvider.Copyright );
			Assert.Equal( assembly.From<DebuggableAttribute, string>( attribute => "DEBUG" ), fromProvider.Configuration );
			Assert.Equal( assembly.From<AssemblyDescriptionAttribute, string>( attribute => attribute.Description ), fromProvider.Description );
			Assert.Equal( assembly.From<AssemblyProductAttribute, string>( attribute => attribute.Product ), fromProvider.Product );
			Assert.Equal( assembly.GetName().Version, fromProvider.Version );
		}

		[Theory, LocationSetup.AutoData]
		void RegisterInterface( IAnotherInterface sut )
		{
			Assert.IsType<MultipleInterfaces>( sut );
		}

		public interface IRegisteredWithName
		{ }

		[Register.Mapped( typeof(IAnotherInterface) )]
		public class MultipleInterfaces : IInterface, IAnotherInterface, IItem
		{}

		interface IAnotherInterface
		{ }

		[Register.Mapped( "Registered" )]
		public class MappedWithNameClass : IRegisteredWithName
		{ }

		class Interfaces
		{
			public Interfaces( IEnumerable<IItem> items )
			{
				Items = items;
			}

			public IEnumerable<IItem> Items { get; }
		}

		public interface IItem
		{ }

		[Register.Mapped]
		public class Item : IItem
		{ }

		[Register.Mapped( "AnotherItem" )]
		public class AnotherItem : IItem
		{ }

		[Register.Mapped( "YetAnotherItem" )]
		public class YetAnotherItem : IItem
		{ }
	}

	[Export]
	public class ServiceLocatorFactory : Activation.IoC.ServiceLocatorFactory
	{
		[ImportingConstructor]
		public ServiceLocatorFactory( Assembly[] assemblies ) : base( new IntegratedUnityContainerFactory( assemblies ).Create ) {}
	}
}
