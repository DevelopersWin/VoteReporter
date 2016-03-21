using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;

namespace DragonSpark.Composition
{
	public class AssemblyBasedContainerConfigurationFactory : AggregateFactory<ContainerConfiguration>
	{
		public AssemblyBasedContainerConfigurationFactory( Func<Assembly[]> primary, params ITransformer<ContainerConfiguration>[] configurations ) : this( new Lazy<Assembly[]>( primary ), configurations ) {}

		AssemblyBasedContainerConfigurationFactory( Lazy<Assembly[]> primary, params ITransformer<ContainerConfiguration>[] configurations )
			: base( 
				ContainerConfigurationFactory.Instance, 
				configurations.Append(
					new AssemblyContainerConfigurationFactory( () => primary.Value ),
					new TypesContainerConfigurationFactory( () => TypesFactory.Instance.Create( primary.Value ) )
					).ToArray()
			)
		{}
	}

	public class CompositionHostFactory : FactoryBase<CompositionHost>
	{
		readonly Func<ContainerConfiguration> configuration;

		public CompositionHostFactory( [Required] Assembly[] assemblies ) : this( assemblies, DefaultLoggingConfigurator.Instance ) {}

		public CompositionHostFactory( [Required] Assembly[] assemblies, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( assemblies.ToFactory(), configurations ) {}

		public CompositionHostFactory( [Required] Func<Assembly[]> assemblies ) : this( assemblies, DefaultLoggingConfigurator.Instance ) {}

		public CompositionHostFactory( [Required] Func<Assembly[]> assemblies, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( new Func<ContainerConfiguration>( new AssemblyBasedContainerConfigurationFactory( assemblies, configurations ).Create ) ) {}

		public CompositionHostFactory( [Required] Type[] types ) : this( types, DefaultLoggingConfigurator.Instance ) {}

		public CompositionHostFactory( [Required] Type[] types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( types.ToFactory(), configurations ) {}

		public CompositionHostFactory( [Required] Func<Type[]> types ) : this( types, DefaultLoggingConfigurator.Instance ) {}

		public CompositionHostFactory( [Required] Func<Type[]> types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( new Func<ContainerConfiguration>( new TypesBasedContainerConfigurationFactory( types, configurations ).Create ) ) {}

		CompositionHostFactory( [Required] Func<ContainerConfiguration> configuration )
		{
			this.configuration = configuration;
		}

		protected override CompositionHost CreateItem() => configuration().CreateContainer();
	}

	public class TypesBasedContainerConfigurationFactory : AggregateFactory<ContainerConfiguration>
	{
		public TypesBasedContainerConfigurationFactory( Func<Type[]> types, params ITransformer<ContainerConfiguration>[] configurations )
			: base( 
				ContainerConfigurationFactory.Instance, 
				configurations.Append( new TypesContainerConfigurationFactory( types ) ).ToArray()
			)
		{}
	}

	public class ContainerConfigurationFactory : FactoryBase<ContainerConfiguration>
	{
		public static ContainerConfigurationFactory Instance { get; } = new ContainerConfigurationFactory();

		protected override ContainerConfiguration CreateItem() => 
			new ContainerConfiguration()
				.WithProvider( new RegisteredExportDescriptorProvider() );
	}

	public abstract class ContainerConfigurator : TransformerBase<ContainerConfiguration> {}

	public class DefaultLoggingConfigurator : ContainerConfigurator
	{
		public static DefaultLoggingConfigurator Instance { get; } = new DefaultLoggingConfigurator();

		protected override ContainerConfiguration CreateItem( ContainerConfiguration parameter ) => parameter.WithProvider( new DefaultLoggingExportDescriptorProvider() );
	}

	public class AssemblyContainerConfigurationFactory : ContainerConfigurator
	{
		readonly Func<Assembly[]> assemblySource;

		public AssemblyContainerConfigurationFactory( [Required] Func<Assembly[]> assemblySource )
		{
			this.assemblySource = assemblySource;
		}

		protected override ContainerConfiguration CreateItem( ContainerConfiguration configuration ) => configuration.WithInstance( assemblySource() );
	}

	public class TypesContainerConfigurationFactory : ContainerConfigurator
	{
		readonly Func<Type[]> typesSource;

		public TypesContainerConfigurationFactory( [Required]Func<Type[]> typesSource )
		{
			this.typesSource = typesSource;
		}

		protected override ContainerConfiguration CreateItem( ContainerConfiguration configuration )
		{
			var types = typesSource();
			var factoryTypes = types.Where( FactoryTypeFactory.Specification.Instance.IsSatisfiedBy ).Select( FactoryTypeFactory.Instance.Create ).ToArray();
			var locator = new DiscoverableFactoryTypeLocator( factoryTypes );
			var conventionLocator = new BuildableTypeFromConventionLocator( types );
			var activator = new CompositeActivator( new SingletonActivator( new SingletonLocator( conventionLocator ) ), SystemActivator.Instance );

			var result = configuration
				.WithParts( types )
				.WithInstance( types )
				.WithInstance( conventionLocator )
				.WithInstance( factoryTypes )
				.WithInstance( locator )
				.WithInstance<IActivator>( activator )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryWithParameterDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryExportDescriptorProvider( locator ) )
				.WithProvider( new TypeInitializingExportDescriptorProvider( conventionLocator ) );
			return result;
		}
	}

	class SingletonActivator : IActivator
	{
		readonly ISingletonLocator locator;

		public SingletonActivator( [Required]ISingletonLocator locator )
		{
			this.locator = locator;
		}

		public bool CanActivate( Type type, string name = null ) => locator.Locate( type ) != null;

		public object Activate( Type type, string name = null ) => locator.Locate( type );

		public bool CanConstruct( Type type, params object[] parameters ) => false;

		public object Construct( Type type, params object[] parameters ) => null;
	}
}