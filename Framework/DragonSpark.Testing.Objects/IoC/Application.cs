using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System;
using System.Composition.Hosting;
using System.Reflection;
using DragonSpark.Activation;
using ServiceProviderCoreFactory = DragonSpark.Activation.IoC.ServiceProviderCoreFactory;

namespace DragonSpark.Testing.Objects.IoC
{
	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : this( provider => new Application( provider ) ) {}

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> applicationSource ) : this( AssemblyProvider.Instance.Create, applicationSource ) {}

		protected AutoDataAttribute( Func<Assembly[]> assemblySource, Func<IServiceProvider, IApplication> applicationSource ) : this( new Factory( assemblySource() ).Create, applicationSource ) {}

		protected AutoDataAttribute( Func<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource ) : base( Providers.From( providerSource, applicationSource ) ) {}

		class Factory : CachedServiceProviderFactory
		{
			public Factory( Assembly[] source ) : base( data => new DragonSpark.Composition.ServiceProviderCoreFactory( new Func<ContainerConfiguration>( new AssemblyBasedConfigurationContainerFactory( source ).Create ) ).Create(), source ) {}

			protected override IServiceProvider CreateItem( AutoData parameter )
			{
				var provider = new Func<IServiceProvider>( new ServiceProviderCoreFactory( () => base.CreateItem( parameter ) ).Create );
				var result = new ServiceProviderFactory( provider ).Create();
				return result;
			}
		}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		public AssemblyProvider( params Type[] others ) : base( others.Append( typeof(AssemblySourceBase) ).Fixed(), DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}
}
