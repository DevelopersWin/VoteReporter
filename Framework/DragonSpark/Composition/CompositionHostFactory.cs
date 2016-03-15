using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using DragonSpark.Setup;

namespace DragonSpark.Composition
{
	/*public static class Composer
	{
		// public static CompositionHost Current => new CurrentApplication().Item.Context.Get<CompositionHost>();

		// public static T Compose<T>() => (T)Compose( typeof(T) );

		// public static object Compose( [Required] Type type ) => Current.GetExport( type );

		public static object ComposeMany( [Required] Type type ) => Current.GetExports( type );
	}

	public class CompositionHostContext : ExecutionContextValue<CompositionHost> {}*/

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
			protected override bool Verify( Type parameter ) => base.Verify( parameter ) && Factory.IsFactory( parameter ) && parameter.IsDefined<ExportAttribute>();
		}

		protected override FactoryType CreateItem( Type parameter ) => new FactoryType( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Factory.GetResultType( parameter ) );
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
			var factoryTypes = types.Select( FactoryTypeFactory.Instance.Create ).ToArray();
			var locator = new DiscoverableFactoryTypeLocator( factoryTypes );
			var result = configuration()
				.WithParts( types )
				.WithProvider( TypeInitializingExportDescriptorProvider.Instance )
				.WithProvider( new RegisteredExportDescriptorProvider() )
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
	}
}
