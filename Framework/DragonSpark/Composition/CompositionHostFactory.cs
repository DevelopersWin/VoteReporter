using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public static class Composer
	{
		public static CompositionHost Current => new CompositionHostContext().Item;

		public static T Compose<T>() => (T)Compose( typeof(T) );

		public static object Compose( [Required] Type type ) => Current.GetExport( type );

		public static object ComposeMany( [Required] Type type ) => Current.GetExports( type );
	}

	public class CompositionHostContext : ExecutionContextValue<CompositionHost> {}

	public class FactoryTypeProfile
	{
		public FactoryTypeProfile( [Required]Type factoryType, string name, [Required]Type resultType )
		{
			FactoryType = factoryType;
			Name = name;
			ResultType = resultType;
		}

		public Type FactoryType { get; }
		public string Name { get; }
		public Type ResultType { get; }
	}

	public class FactoryTypeContainer : List<FactoryTypeProfile>
	{
		public FactoryTypeContainer( IEnumerable<Type> types ) : base( 
			types
				.Where( Factory.IsFactory )
				.Where( x => x.IsDefined<ExportAttribute>( true ) )
				.Where( CanBuildSpecification.Instance.IsSatisfiedBy )
				.Select( type => new FactoryTypeProfile( type, type.From<ExportAttribute, string>( attribute => attribute.ContractName ), Factory.GetResultType( type ) ) ) )
		{}
	}

	public class CompositionHostFactory : FactoryBase<Assembly[], CompositionHost>
	{
		readonly Func<ContainerConfiguration> configuration;
		public static CompositionHostFactory Instance { get; } = new CompositionHostFactory();

		public CompositionHostFactory() : this( () => new ContainerConfiguration() ) {}

		public CompositionHostFactory( Func<ContainerConfiguration> configuration )
		{
			this.configuration = configuration;
		}

		protected override CompositionHost CreateItem( Assembly[] parameter )
		{
			var types = parameter.SelectMany( assembly => assembly.DefinedTypes ).AsTypes().ToArray();
			var container = new FactoryTypeContainer( types );
			var locator = new DiscoverableFactoryTypeLocator( container );
			var result = configuration()
				.WithParts( types )
				.WithProvider( new RegisteredExportDescriptorProvider() )
				.WithInstance( parameter )
				.WithInstance( types )
				.WithInstance( container )
				.WithInstance( locator )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryWithParameterDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryExportDescriptorProvider( locator ) )
				.CreateContainer();
			return result;
		}
	}
}
