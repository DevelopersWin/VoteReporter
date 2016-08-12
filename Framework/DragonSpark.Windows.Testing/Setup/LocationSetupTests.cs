using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Setup.Registration;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.IoC;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using DragonSpark.Testing.Objects.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using DragonSpark.Windows.Testing.TestObjects;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Serilog;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Activator = DragonSpark.Activation.Activator;
using ApplicationAssemblyLocator = DragonSpark.Windows.Runtime.ApplicationAssemblyLocator;
using Attribute = DragonSpark.Testing.Objects.Attribute;
using Locator = DragonSpark.Windows.Testing.TestObjects.Locator;
using Object = DragonSpark.Testing.Objects.Object;
using ServiceLocator = DragonSpark.Activation.IoC.ServiceLocator;

namespace DragonSpark.Windows.Testing.Setup
{
	/// <summary>
	/// This file can be seen as a bucket for all the testing done around setup.  It also can be seen as a huge learning bucket for xUnit and AutoFixture.  This does not contain best practices.  Always be learning. :)
	/// </summary>
	[Trait( Traits.Category, Traits.Categories.IoC ), ContainingTypeAndNested, FrameworkTypes, IoCTypes, AdditionalTypes( typeof(LocationSetup) )]
	public class LocationSetupTests : TestCollectionBase
	{
		public LocationSetupTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void CoreLocation( [Service]IServiceLocator sut )
		{
			Assert.True( Microsoft.Practices.ServiceLocation.ServiceLocator.IsLocationProviderSet );
			Assert.Same( sut, Microsoft.Practices.ServiceLocation.ServiceLocator.Current );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void SetupRegistered( ISetup sut )
		{
			Assert.IsType<LocationSetup>( sut );
		}

		[Map( typeof(IActivator), typeof(Locator))]
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void CreateInstance( [Registered]IActivator locator )
		{
			var expected = GlobalServiceProvider.GetService<IActivator>();
			Assert.Same( expected, locator );
			Assert.NotSame( Activator.Instance.Get(), locator );
			Assert.IsType<Locator>( locator );
			var instance = locator.Activate<IObject>( typeof(Object) );
			Assert.IsType<Object>( instance );

			Assert.Equal( "DefaultActivation", instance.Name );
		}

		[Map( typeof(IActivator), typeof(Locator) )]
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void CreateNamedInstance( [Registered]IActivator activator, string name )
		{
			Assert.Same( GlobalServiceProvider.GetService<IActivator>(), activator );
			Assert.NotSame( Activator.Instance.Get(), activator );

			var instance = activator.Activate<IObject>( new LocateTypeRequest( typeof(Object), name ) );
			Assert.IsType<Object>( instance );

			Assert.Equal( name, instance.Name );
		}

		[Map( typeof(IActivator), typeof(TestObjects.Constructor) )]
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void CreateItem( [Registered]IActivator activator )
		{
			var parameters = new object[] { typeof(Object), "This is Some Name." };
			Assert.Same( GlobalServiceProvider.GetService<IActivator>(), activator );
			Assert.NotSame( Activator.Instance.Get(), activator );
			var instance = activator.Construct<DragonSpark.Testing.Objects.Item>( parameters );
			Assert.NotNull( instance );

			Assert.Equal( parameters, instance.Parameters );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void RegisterInstanceGeneric( IServiceRegistry registry, Class instance )
		{
			registry.Register<IInterface>( instance );

			var located = GlobalServiceProvider.GetService<IInterface>();
			Assert.IsType<Class>( located );
			Assert.Equal( instance, located );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void RegisterGeneric()
		{
			var registry = GlobalServiceProvider.GetService<IServiceRegistry>();
			registry.Register<IInterface, Class>();

			var located = GlobalServiceProvider.GetService<IInterface>();
			Assert.IsType<Class>( located );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void RegisterLocation()
		{
			var registry = GlobalServiceProvider.GetService<IServiceRegistry>();
			registry.Register<IInterface, Class>();

			var located = GlobalServiceProvider.GetService<IInterface>();
			Assert.IsType<Class>( located );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void RegisterInstanceClass( Class instance )
		{
			var registry = GlobalServiceProvider.GetService<IServiceRegistry>();
			registry.Register<IInterface>( instance );

			var located = GlobalServiceProvider.GetService<IInterface>();
			Assert.IsType<Class>( located );
			Assert.Equal( instance, located );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void RegisterFactoryClass( Class instance )
		{
			var registry = GlobalServiceProvider.GetService<IServiceRegistry>();
			registry.Register<IInterface>( () => instance );

			var located = GlobalServiceProvider.GetService<IInterface>();
			Assert.IsType<Class>( located );
			Assert.Equal( instance, located );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void With( IServiceLocator locator, [Frozen, Registered]ClassWithParameter instance )
		{
			var item = locator.GetInstance<ClassWithParameter>().With( x => x.Parameter );
			Assert.Equal( instance.Parameter, item );

			Assert.Null( locator.GetInstance<IInterface>().With( x => x ) );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void WithDefault()
		{
			var item = GlobalServiceProvider.GetService<ClassWithParameter>().With( x => x.Parameter != null );
			Assert.True( item );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void RegisterWithRegistry( Mock<IServiceRegistry> sut )
		{
			var registry = GlobalServiceProvider.GetService<IServiceRegistry>();
			registry.Register( sut.Object );

			var registered = GlobalServiceProvider.GetService<IServiceRegistry>();
			Assert.Same( sut.Object, registered );
			registered.Register<IInterface, Class>();

			sut.Verify( x => x.Register( It.Is<MappingRegistrationParameter>( parameter => parameter.MappedTo == typeof(Class) && parameter.RequestedType == typeof(IInterface) && parameter.Name == null ) ) );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void Resolve( [Service]Interfaces sut )
		{
			Assert.NotNull( sut.Items.FirstOrDefaultOfType<Item>() );
			Assert.NotNull( sut.Items.FirstOrDefaultOfType<AnotherItem>() );
			Assert.NotNull( sut.Items.FirstOrDefaultOfType<YetAnotherItem>() );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void GetInstance( [Frozen( Matching.ExactType ), Service]ServiceLocator sut, [Frozen, Registered]Mock<ILogger> logger )
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

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void GetAllInstancesLocator( [Modest, Service] ServiceLocator sut )
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

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
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

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void Register( IUnityContainer container, [Greedy, Frozen]ServiceLocator sut )
		{
			var registry = sut.GetInstance<IServiceRegistry>();
			Assert.False( container.IsRegistered<IInterface>() );
			registry.Register<IInterface, Class>();
			Assert.True( container.IsRegistered<IInterface>() );
			Assert.IsType<Class>( container.Resolve<IInterface>() );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
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

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void RegisterFactory( IUnityContainer container, [Greedy, Frozen]ServiceLocator sut )
		{
			Assert.False( container.IsRegistered<IInterface>() );
			var registry = sut.GetInstance<IServiceRegistry>();
			registry.RegisterFactory( new FactoryRegistrationParameter( typeof(IInterface), () => new Class() ) );
			Assert.True( container.IsRegistered<IInterface>() );
			Assert.IsType<Class>( container.Resolve<IInterface>() );
		}

		/*[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void Dispose( [DragonSpark.Testing.Framework.Category, Frozen] ServiceLocator sut )
		{
			var item = Services.Get<IInterface>( typeof(Class) );
			Assert.Exists( item );

			var disposable = new Disposable();

			var registry = sut.GetInstance<IServiceRegistry>();
			registry.Register( disposable );
			// registry.Register( new ServiceLocationMonitor( Services.Location, sut ) );

			Assert.False( disposable.Disposed );

			// Assert.Same( Services.Location.Item, sut );

			sut.QueryInterface<IDisposable>().Dispose();

			// Assert.NotSame( Services.Location.Item, sut );

			Assert.True( disposable.Disposed );
		}*/

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void RelayedPropertyAttribute( IUnityContainer container )
		{
			var attribute = typeof(Relayed).GetProperty( nameof(Relayed.Property) ).GetAttribute<Attribute>();
			Assert.Equal( "This is a relayed property attribute.", attribute.PropertyName );
		}


		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		void RelayedAttribute()
		{
			var attribute = typeof(Relayed).GetAttribute<Attribute>();
			Assert.Equal( "This is a relayed class attribute.", attribute.PropertyName );
		}

		/*[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		[Map(typeof(IExceptionFormatter), typeof(ExceptionFormatter) )]
		public void Try()
		{
			var exception = ExceptionSupport.Try( Delegates.Empty );
			Assert.Null( exception );
		}*/

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, IncludeParameterTypes( typeof(NormalPriority) )]
		public void GetAllTypesWith( [Service] ImmutableArray<System.Type> sut )
		{
			Assert.True( sut.Decorated<PriorityAttribute>().Contains( typeof(NormalPriority) ) );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void Evaluate( ClassWithParameter sut )
		{
			Assert.Equal( sut.Parameter, sut.Evaluate<object>( nameof(sut.Parameter) ) );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void Mocked( [Frozen]Mock<IInterface> sut, IInterface item )
		{
			Assert.Equal( sut.Object, item );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void GetAllInstances( IServiceLocator sut )
		{
			Assert.False( sut.GetAllInstances<Class>().Any() );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, IncludeParameterTypes( typeof(RegisterAsSingleton) )]
		public void Singleton( IUnityContainer sut )
		{
			var once = sut.Resolve<RegisterAsSingleton>();
			var twice = sut.Resolve<RegisterAsSingleton>();
			Assert.Same( once, twice );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, IncludeParameterTypes( typeof(RegisterAsMany) )]
		public void Many( IUnityContainer sut )
		{
			var once = sut.Resolve<RegisterAsMany>();
			var twice = sut.Resolve<RegisterAsMany>();
			Assert.NotSame( once, twice );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, IncludeParameterTypes( typeof(YetAnotherClass) )]
		public void Factory( AllTypesOfFactory sut )
		{
			var items = sut.Create<IInterface>();
			Assert.True( items.Any() );
			Assert.NotNull( items.FirstOrDefaultOfType<YetAnotherClass>() );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void Locate( [Service]Assembly[] assemblies,  [Service]ApplicationAssemblyLocator sut )
		{
			Assert.Same( GetType().Assembly, sut.Get( assemblies ) );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void RegisterWithName( IServiceLocator locator )
		{
			Assert.Null( locator.GetInstance<IRegisteredWithName>() );

			var located = locator.GetInstance<IRegisteredWithName>( "Registered" );
			Assert.IsType<MappedWithNameClass>( located );
		}

		/*[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void CreateAssemblies( IUnityContainer container, ApplicationTypes provider, [Service]ImmutableArray<Assembly> sut )
		{
			var registered = container.IsRegistered<ImmutableArray<Assembly>>();
			Assert.True( registered );

			var fromContainer = container.Resolve<IEnumerable<Assembly>>().Fixed();
			var fromProvider = provider.Get();
			var assemblies = fromProvider.Assemblies().ToArray();
			Assert.Equal( fromContainer, assemblies );

			Assert.Equal( fromContainer, sut );
		}*/
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void CreateAssemblies( IUnityContainer container, ApplicationTypes provider, ImmutableArray<Assembly> sut )
		{
			var registered = container.IsRegistered<ImmutableArray<Assembly>>();
			Assert.True( registered );

			var fromContainer = container.Resolve<ImmutableArray<Assembly>>();
			var fromProvider = provider.Get();
			var assemblies = fromProvider.Assemblies().ToArray();
			Assert.Equal( fromContainer.ToArray(), assemblies );

			Assert.Equal( fromContainer, sut );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void ConventionLocator( ConventionTypes locator )
		{
			var type = locator.Get( typeof(Assembly) );
			Assert.Null( type );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, IncludeParameterTypes( typeof(ApplicationAssembly) )]
		public void CreateAssemblySimple( IUnityContainer container, [Service]Assembly sut )
		{
			Assert.True( container.IsRegistered<Assembly>() );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, IncludeParameterTypes( typeof(ApplicationAssembly) )]
		public void EnsureActivatorResolvesAsExpected( [Service]IActivator activator )
		{
			var assembly = activator.Activate<Assembly>();
			Assert.NotNull( assembly );
			Assert.Equal( GetType().Assembly, assembly );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, AdditionalTypes( typeof(ApplicationAssembly) )]
		public void BasicComposition( IUnityContainer container )
		{
			using ( var host = CompositionHostFactory.Instance.Create() )
			{
				var assembly = host.GetExport<Assembly>();
				Assert.NotNull( assembly );
				Assert.Equal( GetType().Assembly, assembly );
			}
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, IncludeParameterTypes( typeof(ApplicationAssembly) )]
		public void CreateAssembly( [Service]AssemblyInformationFactory factory, IUnityContainer container, [Service]Assembly sut )
		{
			var fromFactory = ApplicationAssembly.Instance.Get();
			var fromContainer = container.Resolve<Assembly>();
			Assert.Same( fromFactory, fromContainer );
			

			Assert.Same( fromContainer, sut );

			var fromProvider = factory.Create( fromFactory );
			Assert.NotNull( fromProvider );

			var assembly = GetType().Assembly;
			Assert.Equal( AttributeProviderExtensions.From<AssemblyTitleAttribute, string>( assembly, attribute => attribute.Title ), fromProvider.Title );
			Assert.Equal( AttributeProviderExtensions.From<AssemblyCompanyAttribute, string>( assembly, attribute => attribute.Company ), fromProvider.Company );
			Assert.Equal( AttributeProviderExtensions.From<AssemblyCopyrightAttribute, string>( assembly, attribute => attribute.Copyright ), fromProvider.Copyright );
			Assert.Equal( AttributeProviderExtensions.From<DebuggableAttribute, string>( assembly, attribute => "DEBUG" ), fromProvider.Configuration );
			Assert.Equal( AttributeProviderExtensions.From<AssemblyDescriptionAttribute, string>( assembly, attribute => attribute.Description ), fromProvider.Description );
			Assert.Equal( AttributeProviderExtensions.From<AssemblyProductAttribute, string>( assembly, attribute => attribute.Product ), fromProvider.Product );
			Assert.Equal( assembly.GetName().Version, fromProvider.Version );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData, IncludeParameterTypes( typeof(MultipleInterfaces) )]
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

		public class Interfaces
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

	/*[Export]
	public class ServiceLocatorFactory : FactoryBase<IServiceLocator>
	{
		readonly Func<IServiceProvider> source;

		[ImportingConstructor]
		public ServiceLocatorFactory( Assembly[] assemblies ) : this( new AssemblyBasedServiceProviderFactory( assemblies ).Create ) {}

		public ServiceLocatorFactory( Func<IServiceProvider> source )
		{
			this.source = source;
		}

		public override IServiceLocator Create()
		{
			var provider = source();
			var result = provider.Get<IServiceLocator>();
			return result;
		}
	}*/
}
