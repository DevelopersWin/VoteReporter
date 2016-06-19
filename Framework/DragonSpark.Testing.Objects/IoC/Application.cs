using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Testing.Objects.IoC
{
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		public UnityContainerFactory() : base( new DragonSpark.Setup.AssemblyBasedServiceProviderFactory( Items<Assembly>.Default ).Create() ) {}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		readonly static DelegatedFactory<IServiceProvider, IApplication> ApplicationSource = new DelegatedFactory<IServiceProvider, IApplication>( provider => new Application( provider ) );

		public AutoDataAttribute() : this( ApplicationSource ) {}

		protected AutoDataAttribute( IFactory<IServiceProvider, IApplication> applicationSource ) : this( AssemblyProvider.Instance, applicationSource ) {}

		protected AutoDataAttribute( IFactory<Assembly[]> assemblySource, IFactory<IServiceProvider, IApplication> applicationSource ) : base( new Context( assemblySource, applicationSource ) ) {}

		class Context : FactoryWithSpecificationBase<AutoData, IDisposable>
		{
			readonly IFactory<Assembly[]> assemblySource;
			readonly IFactory<IServiceProvider, IApplication> applicationSource;

			public Context( IFactory<Assembly[]> assemblySource, IFactory<IServiceProvider, IApplication> applicationSource )
			{
				this.assemblySource = assemblySource;
				this.applicationSource = applicationSource;
			}

			public override IDisposable Create( AutoData parameter )
			{
				var cached = new Cache( assemblySource.Create() ).Create( parameter );
				var provider = new Activation.IoC.ServiceProviderFactory( cached.ToFactory() ).Create();
				var result = new AutoDataExecutionContextFactory( provider.Wrap<AutoData, IServiceProvider>(), applicationSource ).Create( parameter );
				return result;
			}
		}

		class Cache : CacheFactoryBase
		{
			readonly Assembly[] assemblies;
			public Cache( Assembly[] assemblies ) : base( new AssemblyBasedServiceProviderFactory( assemblies ).Wrap<AutoData, IServiceProvider>() )
			{
				this.assemblies = assemblies;
			}

			protected override ImmutableArray<object> GetKeyItems( AutoData parameter ) => assemblies.ToImmutableArray<object>();
		}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		public AssemblyProvider( params Type[] others ) : base( others.Append( typeof(AssemblySourceBase) ).Fixed(), DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}
}
