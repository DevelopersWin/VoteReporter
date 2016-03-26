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
using DragonSpark.Runtime.Specifications;
using Constructor = DragonSpark.Activation.Constructor;

namespace DragonSpark.Composition
{
	public class CompositionHostFactory : FactoryBase<CompositionHost>
	{
		readonly Func<ContainerConfiguration> configuration;

		public CompositionHostFactory( [Required] Assembly[] assemblies ) : this( assemblies, DefaultLoggingConfigurator.Instance ) {}

		public CompositionHostFactory( [Required] Assembly[] assemblies, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( assemblies.ToFactory(), () => TypesFactory.Instance.Create( assemblies ), configurations ) {}

		public CompositionHostFactory( [Required] Func<Assembly[]> assemblies ) : this( assemblies, DefaultLoggingConfigurator.Instance ) {}

		public CompositionHostFactory( [Required] Func<Assembly[]> assemblies, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( new Lazy<Assembly[]>( assemblies ), configurations ) {}

		CompositionHostFactory( [Required] Lazy<Assembly[]> assemblies, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( () => assemblies.Value, () => TypesFactory.Instance.Create( assemblies.Value), configurations ) {}

		public CompositionHostFactory( [Required] Type[] types ) : this( types, DefaultLoggingConfigurator.Instance ) {}

		public CompositionHostFactory( [Required] Type[] types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( AssembliesFactory.Instance.Create( types ).ToFactory(), types.ToFactory(), configurations ) {}

		public CompositionHostFactory( [Required] Func<Type[]> types ) : this( types, DefaultLoggingConfigurator.Instance ) {}

		public CompositionHostFactory( [Required] Func<Type[]> types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( new System.Lazy<Type[]>( types ), configurations ) {}

		CompositionHostFactory( [Required] Lazy<Type[]> types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( () => AssembliesFactory.Instance.Create( types.Value ), () => types.Value, configurations ) {}

		public CompositionHostFactory( [Required] Func<Assembly[]> assemblies, [Required] Func<Type[]> types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) : this( new Func<ContainerConfiguration>( new ContainerConfigurationFactory( assemblies, types, configurations ).Create ) ) {}

		CompositionHostFactory( [Required] Func<ContainerConfiguration> configuration )
		{
			this.configuration = configuration;
		}

		protected override CompositionHost CreateItem() => configuration().CreateContainer();
	}

	public class ContainerConfigurationFactory : AggregateFactory<ContainerConfiguration>
	{
		public ContainerConfigurationFactory( [Required] Func<Assembly[]> assemblies, [Required] Func<Type[]> types, params ITransformer<ContainerConfiguration>[] configurations )
			: base( 
				ContainerConfigurationCoreFactory.Instance, 
				configurations.Append( new PartsContainerConfigurationFactory( assemblies, types ) ).ToArray()
			)
		{}
	}

	public class ContainerConfigurationCoreFactory : FactoryBase<ContainerConfiguration>
	{
		public static ContainerConfigurationCoreFactory Instance { get; } = new ContainerConfigurationCoreFactory();

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
		readonly Func<Assembly[]> assemblySource;
		readonly Func<Type[]> typesSource;

		public PartsContainerConfigurationFactory( [Required] Func<Assembly[]> assemblySource, [Required]Func<Type[]> typesSource )
		{
			this.assemblySource = assemblySource;
			this.typesSource = typesSource;
		}

		protected override ContainerConfiguration CreateItem( ContainerConfiguration configuration )
		{
			var assemblies = assemblySource();
			var types = typesSource();
			var factoryTypes = types.Where( FactoryTypeFactory.Specification.Instance.IsSatisfiedBy ).Select( FactoryTypeFactory.Instance.Create ).ToArray();
			var locator = new DiscoverableFactoryTypeLocator( factoryTypes );
			var conventionLocator = new BuildableTypeFromConventionLocator( types );
			var activator = new CompositeActivator( new SingletonLocator( conventionLocator ), Constructor.Instance );

			var result = configuration
				.WithParts( types.Union( new []{ typeof(ConfigureProviderCommand.Context) } )/*, AttributeProvider.Instance*/ )
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

		/*class AttributeProvider : AttributedModelProvider
		{
			public static AttributeProvider Instance { get; } = new AttributeProvider();

			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, MemberInfo member ) => member.GetAttributes<Attribute>();

			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, ParameterInfo parameter ) => parameter.GetAttributes<Attribute>();
		}*/
	}

	class SingletonLocator : LocatorBase
	{
		readonly Func<Type, Type> convention;
		readonly ISingletonLocator singleton;

		public SingletonLocator( [Required] BuildableTypeFromConventionLocator convention ) : this( convention.Create, Activation.IoC.SingletonLocator.Instance ) {}

		public SingletonLocator( [Required] Func<Type, Type> convention, [Required]ISingletonLocator singleton ) : base( new Specification( convention, singleton ).Wrap<LocateTypeRequest>( request => request.RequestedType ) )
		{
			this.convention = convention;
			this.singleton = singleton;
		}

		class Specification : ContainsSingletonSpecification
		{
			readonly Func<Type, Type> convention;

			public Specification( Func<Type, Type> convention, ISingletonLocator locator ) : base( locator )
			{
				this.convention = convention;
			}

			protected override bool Verify( Type parameter )
			{
				var type = convention( parameter ) ?? parameter;
				var result = base.Verify( type );
				return result;
			}
		}

		protected override object CreateItem( LocateTypeRequest parameter )
		{
			var type = convention( parameter.RequestedType ) ?? parameter.RequestedType;
			var result = singleton.Locate( type );
			return result;
		}
	}
}