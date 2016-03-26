using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using DragonSpark.Activation;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public class FactoryType
	{
		public FactoryType( [Required]Type runtimeType, string name, [Required]Type resultType )
		{
			RuntimeType = runtimeType;
			Name = name;
			ResultType = resultType;
		}

		public Type RuntimeType { get; }
		public string Name { get; }
		public Type ResultType { get; }
	}

	public class FactoryTypeFactory : FactoryBase<Type, FactoryType>
	{
		public static FactoryTypeFactory Instance { get; } = new FactoryTypeFactory( Specification.Instance );

		public FactoryTypeFactory( ISpecification<Type> specification ) : base( specification ) {}

		public class Specification : CanBuildSpecification
		{
			public new static Specification Instance { get; } = new Specification();

			[Freeze]
			protected override bool Verify( Type parameter ) => base.Verify( parameter ) && Factory.IsFactory( parameter ) && parameter.Adapt().IsDefined<ExportAttribute>();
		}

		protected override FactoryType CreateItem( Type parameter ) => new FactoryType( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Factory.GetResultType( parameter ) );
	}

	public class AssembliesFactory : FactoryBase<Type[], Assembly[]>
	{
		public static AssembliesFactory Instance { get; } = new AssembliesFactory();

		[Freeze]
		protected override Assembly[] CreateItem( Type[] parameter ) => parameter.Assemblies().Distinct().ToArray();
	}

	public static class AssemblyTypes
	{
		public static AssemblyTypesFactory All { get; } = new AssemblyTypesFactory( assembly => assembly.DefinedTypes.AsTypes() );

		public static AssemblyTypesFactory Public { get; } = new AssemblyTypesFactory( assembly => assembly.ExportedTypes );
	}

	public class AssemblyTypesFactory : FactoryBase<Assembly, Type[]>
	{
		readonly Func<Assembly, IEnumerable<Type>> types;

		public AssemblyTypesFactory( [Required] Func<Assembly, IEnumerable<Type>> types )
		{
			this.types = types;
		}

		[Freeze]
		protected override Type[] CreateItem( Assembly parameter ) => types( parameter ).Fixed();
	}

	public class TypesFactory : FactoryBase<Assembly[], Type[]>
	{
		public static TypesFactory Instance { get; } = new TypesFactory();

		[Freeze]
		protected override Type[] CreateItem( Assembly[] parameter ) => parameter.SelectMany( AssemblyTypes.All.Create ).ToArray();
	}

	public class ServiceProviderFactory : Setup.ServiceProviderFactory
	{
		public ServiceProviderFactory( [Required] Func<CompositionHost> source ) : base( new ServiceLocatorFactory( source ).Create, ConfigureProviderCommand.Instance.Run ) {}
	}

	public class ServiceLocatorFromPartsFactory : ServiceLocatorFactory
	{
		public ServiceLocatorFromPartsFactory( [Required]Assembly[] assemblies ) : this( assemblies.ToFactory() ) {}

		public ServiceLocatorFromPartsFactory( Func<Assembly[]> assemblies ) : this( new CompositionHostFactory( assemblies ) ) {}

		public ServiceLocatorFromPartsFactory( [Required]Type[] types ) : this( types.ToFactory() ) {}

		public ServiceLocatorFromPartsFactory( Func<Type[]> types ) : this( new CompositionHostFactory( types ) ) {}

		public ServiceLocatorFromPartsFactory( CompositionHostFactory factory ) : base( factory.Create ) {}
	}

	public class ServiceLocatorFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<CompositionHost> source;

		public ServiceLocatorFactory( [Required] Func<CompositionHost> source )
		{
			this.source = source;
		}

		protected override IServiceProvider CreateItem() => new ServiceLocator( source() );
	}

	public sealed class ConfigureProviderCommand : ConfigureProviderCommandBase<ConfigureProviderCommand.Context>
	{
		public static ConfigureProviderCommand Instance { get; } = new ConfigureProviderCommand();

		[Export]
		public sealed class Context
		{
			[ImportingConstructor]
			public Context( [Required]IExportDescriptorProviderRegistry registry, [Required]ILogger logger )
			{
				Registry = registry;
				Logger = logger;
			}

			public IExportDescriptorProviderRegistry Registry { get; }

			public ILogger Logger { get; }
		}
		
		protected override void Configure( ProviderContext context )
		{
			context.Context.Logger.Information( Resources.ConfiguringServiceLocatorSingleton );
			context.Provider.As<IServiceLocator>( locator => context.Context.Registry.Register( new InstanceExportDescriptorProvider<IServiceLocator>( locator ) ) );
		}
	}

	public class ServiceLocator : ServiceLocatorImplBase
	{
		readonly CompositionHost host;

		public ServiceLocator( [Required]CompositionHost host )
		{
			this.host = host;
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => host.GetExports( serviceType, null );

		protected override object DoGetInstance(Type serviceType, string key) => serviceType.Adapt().IsInstanceOfType( host ) ? host : Retrieve( serviceType, key );

		object Retrieve( Type serviceType, string key )
		{
			object item;
			var result = host.TryGetExport( serviceType, key, out item ) ? item : null;
			return result;
		}
	}

	/*public class CompositionHostFactory : FactoryBase<Assembly[], CompositionHost>
	{
		public static CompositionHostFactory Instance { get; } = new CompositionHostFactory();

		readonly Func<ContainerConfiguration> factory;

		public CompositionHostFactory() : this( () => new ContainerConfiguration() ) {}

		public CompositionHostFactory( [Required]Func<ContainerConfiguration> factory )
		{
			this.factory = factory;
		}

		protected override CompositionHost CreateItem( Assembly[] parameter )
		{
			//var assemblies = parameter.Append( GetType().Assembly() ).Distinct().Fixed();
			var types = TypesFactory.Instance.Create( parameter );
			var factoryTypes = types.Where( FactoryTypeFactory.Specification.Instance.IsSatisfiedBy ).Select( FactoryTypeFactory.Instance.Create ).ToArray();
			var locator = new DiscoverableFactoryTypeLocator( factoryTypes );
			var conventionLocator = new BuildableTypeFromConventionLocator( types );
			var result = factory()
				.WithProvider( new TypeInitializingExportDescriptorProvider( conventionLocator ) )
				.WithParts( types )
				.WithProvider( new RegisteredExportDescriptorProvider() )
				.WithInstance( conventionLocator )
				.WithInstance( parameter )
				.WithInstance( types )
				.WithInstance( factoryTypes )
				.WithInstance( locator )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryWithParameterDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryExportDescriptorProvider( locator ) )
				.CreateContainer();
			return result;
		}
	}*/
}
