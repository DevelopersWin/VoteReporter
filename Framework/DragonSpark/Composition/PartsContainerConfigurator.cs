using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public class CompositionFactory : FactoryBase<CompositionHost>
	{
		readonly Func<ContainerConfiguration> configuration;
		
		public CompositionFactory( [Required] Func<ContainerConfiguration> configuration )
		{
			this.configuration = configuration;
		}

		protected override CompositionHost CreateItem() => configuration().CreateContainer();
	}

	public class AssemblyBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		// public AssemblyBasedConfigurationContainerFactory( [Required] Assembly[] assemblies ) : this( assemblies, DefaultLoggingConfigurator.Instance ) {}

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
		// public TypeBasedConfigurationContainerFactory( [Required] Type[] types ) : this( types, DefaultLoggingConfigurator.Instance ) {}

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
				configurations.Append( new PartsContainerConfigurator( assemblies, types ) ).ToArray()
			) {}
	}

	public class ContainerConfigurationFactory : FactoryBase<ContainerConfiguration>
	{
		public static ContainerConfigurationFactory Instance { get; } = new ContainerConfigurationFactory();

		protected override ContainerConfiguration CreateItem() => new ContainerConfiguration()
				.WithProvider( new ServicesExportDescriptorProvider() );
	}

	public abstract class ContainerConfigurator : TransformerBase<ContainerConfiguration> {}

	/*public class DefaultLoggingConfigurator : ContainerConfigurator
	{
		public static DefaultLoggingConfigurator Instance { get; } = new DefaultLoggingConfigurator();

		protected override ContainerConfiguration CreateItem( ContainerConfiguration parameter ) => parameter.WithProvider( new DefaultLoggingExportDescriptorProvider() );
	}*/

	public class ServicesExportDescriptorProvider : ExportDescriptorProvider
	{
		readonly Func<IServiceProvider> provider;

		public ServicesExportDescriptorProvider() : this( Services.Get<IServiceProvider> ) {}

		public ServicesExportDescriptorProvider( [Required]Func<IServiceProvider> provider )
		{
			this.provider = provider;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Context( provider(), contract ).Create );
			}
		}

		class Context
		{
			readonly IServiceProvider provider;
			readonly CompositionContract contract;

			public Context( [Required]IServiceProvider provider, [Required]CompositionContract contract )
			{
				this.provider = provider;
				this.contract = contract;
			}

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( Activate, NoMetadata );

			object Activate( LifetimeContext context, CompositionOperation operation ) => provider.GetService( contract.ContractType );
		}
	}

	// [Persistent]
	/*public class ServicesCoordinator
	{
		readonly IServiceProvider provider;
		readonly CompositionContext context;

		public ServicesCoordinator( [Required]IServiceProvider provider, [Required]CompositionContext context )
		{
			this.provider = provider;
			this.context = context;
		}

		public object Create( CompositionContract contract )
		{
			var current = Ambient.GetCurrent<LocateTypeRequest>();
			var result = current.With( operation => new LocateTypeRequest( contract.ContractType, contract.ContractName ) != current, () => true ) ? provider.GetService( contract.ContractType ) : null;
			return result;
		}

		public object Create( LocateTypeRequest request )
		{
			var chain = new ThreadAmbientChain<LocateTypeRequest>();
			if ( !chain.Item.Contains( request ) )
			{
				using ( new AmbientContextCommand<LocateTypeRequest>( chain ).ExecuteWith( request )  )
				{
					var result = context.TryGet<object>( request.RequestedType, request.Name );
					return result;
				}
			}
			return null;
		}
	}*/

	public class PartsContainerConfigurator : ContainerConfigurator
	{
		readonly Assembly[] assemblies;
		readonly Type[] types;

		public PartsContainerConfigurator( [Required] Assembly[] assemblies, [Required]Type[] types )
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
				.WithParts( types, AttributeProvider.Instance )
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