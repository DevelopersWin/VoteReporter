using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public class CompositionHostFactory : FactoryBase<CompositionHost>
	{
		readonly Func<ContainerConfiguration> configuration;
		
		public CompositionHostFactory( [Required] Func<ContainerConfiguration> configuration )
		{
			this.configuration = configuration;
		}

		protected override CompositionHost CreateItem() => configuration().CreateContainer();
	}

	public class AssemblyBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		public AssemblyBasedConfigurationContainerFactory( [Required] Assembly[] assemblies ) : this( assemblies, DefaultLoggingConfigurator.Instance ) {}

		public AssemblyBasedConfigurationContainerFactory( [Required] Assembly[] assemblies, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : base( assemblies, TypesFactory.Instance.Create( assemblies ), configurations ) {}

		/*public AssemblyBasedConfigurationContainerFactory( [Required] Func<Assembly[]> assemblySource ) : this( assemblySource, DefaultLoggingConfigurator.Instance ) {}

		public AssemblyBasedConfigurationContainerFactory( [Required] Func<Assembly[]> assemblySource, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( new Lazy<Assembly[]>( assemblySource ), configurations ) {}

		AssemblyBasedConfigurationContainerFactory( [Required] Lazy<Assembly[]> assemblies, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( () => assemblies.Value, () => TypesFactory.Instance.Create( assemblies.Value), configurations ) {}

		AssemblyBasedConfigurationContainerFactory( [Required] Lazy<Type[]> types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) 
			: this( () => AssembliesFactory.Instance.Create( types.Value ), () => types.Value, configurations ) {}

		public AssemblyBasedConfigurationContainerFactory( Func<Assembly[]> assemblySource, Func<Type[]> typeSource, params ITransformer<ContainerConfiguration>[] configurations ) 
			: base( assemblySource, typeSource, configurations ) {}*/
	}

	public class TypeBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		public TypeBasedConfigurationContainerFactory( [Required] Type[] types ) : this( types, DefaultLoggingConfigurator.Instance ) {}

		public TypeBasedConfigurationContainerFactory( [Required] Type[] types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) 
			: base( AssembliesFactory.Instance.Create( types ), types, configurations ) {}

		// public CompositionHostFactory( [Required] Func<Type[]> types ) : this( types, DefaultLoggingConfigurator.Instance ) {}

		/*public TypeBasedConfigurationContainerFactory( [Required] Func<Type[]> types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) 
			: this( new System.Lazy<Type[]>( types ), configurations ) {}

		public TypeBasedConfigurationContainerFactory( Func<Assembly[]> assemblySource, Func<Type[]> typeSource, params ITransformer<ContainerConfiguration>[] configurations ) 
			: base( assemblySource, typeSource, configurations ) {}*/
	}

	/*class AssociatedContainer : AssociatedValue<CompositionHost>
	{
		public AssociatedContainer( ContainerConfiguration instance ) : base( instance, typeof(AssociatedContainer), instance.CreateContainer ) {}
	}

	public class AssociatedKey<T> : ThreadAmbientValue<T>
	{
		public AssociatedKey( Func<T> create, params object[] parameters ) : this( KeyFactory.Instance.CreateUsing( parameters ).ToString(), create ) {}

		public AssociatedKey( string key, Func<T> create = null ) : base( key, create ) {}
	}*/

	public class ContainerConfigurationFromPartsFactory : AggregateFactory<ContainerConfiguration>
	{
		public ContainerConfigurationFromPartsFactory( [Required] Assembly[] assemblies, [Required] Type[] types, params ITransformer<ContainerConfiguration>[] configurations )
			: base( 
				ContainerConfigurationFactory.Instance, 
				configurations.Append( new PartsContainerConfigurationFactory( assemblies, types ) ).ToArray()
			) {}
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

	public class PartsContainerConfigurationFactory : ContainerConfigurator
	{
		readonly Assembly[] assemblies;
		readonly Type[] types;

		public PartsContainerConfigurationFactory( [Required] Assembly[] assemblies, [Required]Type[] types )
		{
			this.assemblies = assemblies;
			this.types = types;
		}

		protected override ContainerConfiguration CreateItem( ContainerConfiguration configuration )
		{
			var factoryTypes = types.Where( FactoryTypeFactory.Specification.Instance.IsSatisfiedBy ).Select( FactoryTypeFactory.Instance.Create ).ToArray();
			var locator = new DiscoverableFactoryTypeLocator( factoryTypes );
			var conventionLocator = new BuildableTypeFromConventionLocator( types );
			var activator = new Activation.Activator( conventionLocator );

			var result = configuration
				.WithParts( types.Union( new []{ typeof(ConfigureProviderCommand.Context) } ), AttributeProvider.Instance )
				.WithInstance( assemblies )
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

		class AttributeProvider : AttributedModelProvider
		{
			public static AttributeProvider Instance { get; } = new AttributeProvider();

			// [Freeze]
			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, MemberInfo member ) => member.GetAttributes<Attribute>();

			// [Freeze]
			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, ParameterInfo parameter ) => parameter.GetAttributes<Attribute>();
		}
	}
}