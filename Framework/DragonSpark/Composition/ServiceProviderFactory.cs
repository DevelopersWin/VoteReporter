using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
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
using Type = System.Type;

namespace DragonSpark.Composition
{
	public class FactoryTypeRequest : LocateTypeRequest
	{
		public FactoryTypeRequest( [Required]Type runtimeType, string name, [Required]Type resultType ) :  base( runtimeType, name )
		{
			ResultType = resultType;
		}

		public Type ResultType { get; }
	}

	public class FactoryTypeFactory : FactoryBase<Type, FactoryTypeRequest>
	{
		public static FactoryTypeFactory Instance { get; } = new FactoryTypeFactory( Specification.Instance );

		public FactoryTypeFactory( ISpecification<Type> specification ) : base( specification ) {}

		public class Specification : CanBuildSpecification
		{
			public new static Specification Instance { get; } = new Specification();

			[Freeze]
			protected override bool Verify( Type parameter ) => base.Verify( parameter ) && Factory.IsFactory( parameter ) && parameter.Adapt().IsDefined<ExportAttribute>();
		}

		protected override FactoryTypeRequest CreateItem( Type parameter ) => new FactoryTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Factory.GetResultType( parameter ) );
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

	public class ServiceProviderFactory : ServiceProviderFactory<ConfigureProviderCommand>
	{
		public ServiceProviderFactory( [Required] Func<ContainerConfiguration> source ) : base( new ServiceProviderSourceFactory( source ).Create ) {}
	}

	public class ServiceProviderSourceFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<CompositionContext> source;

		public ServiceProviderSourceFactory( Func<ContainerConfiguration> configuration ) : this( Composition.Create.Factory( configuration ) ) {}

		public ServiceProviderSourceFactory( [Required] Func<CompositionContext> source )
		{
			this.source = source;
		}

		protected override IServiceProvider CreateItem()
		{
			var primary = new ServiceLocator( source() );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), primary, Services.Current );
			return result;
		}
	}

	[Export]
	public sealed class ConfigureProviderCommand : Command<IServiceProvider>
	{
		readonly ILogger logger;
		
		[ImportingConstructor]
		public ConfigureProviderCommand( [Required]ILogger logger )
		{
			this.logger = logger;
		}

		protected override void OnExecute( IServiceProvider parameter )
		{
			logger.Information( Resources.ConfiguringServiceLocatorSingleton );
			// context.Provider.As<IServiceLocator>( locator => context.Context.Registry.Register( new InstanceExportDescriptorProvider<IServiceLocator>( locator ) ) );
		}
	}

	public class ServiceLocator : ServiceLocatorImplBase
	{
		readonly CompositionContext host;

		public ServiceLocator( [Required]CompositionContext host )
		{
			this.host = host;
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => host.GetExports( serviceType, null );

		protected override object DoGetInstance(Type serviceType, string key) => host.TryGet<object>( serviceType, key );
	}
}
