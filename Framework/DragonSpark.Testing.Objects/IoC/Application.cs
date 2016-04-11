using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Objects.IoC
{
	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : this( provider => new Application( provider ) ) {}

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> applicationSource ) : this( AssemblyProvider.Instance.Create, applicationSource ) {}

		protected AutoDataAttribute( Func<Assembly[]> assemblySource, Func<IServiceProvider, IApplication> applicationSource ) 
			: this( new Factory( assemblySource() ).Create, applicationSource ) {}

		AutoDataAttribute( Func<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource ) : base( Providers.From( providerSource, applicationSource ) ) {}

		class Cache : Framework.Setup.CacheFactoryBase
		{
			public Cache( Assembly[] assemblies ) : base( autoData => new ServiceProviderFactory( assemblies ).Create(), assemblies.Cast<object>().ToArray() ) {}
		}

		class Factory : FactoryBase<AutoData, IServiceProvider>
		{
			readonly Func<AutoData, IServiceProvider> factory;

			public Factory( Assembly[] assemblies ) : this( new Cache( assemblies ).Create ) {}

			Factory( Func<AutoData, IServiceProvider> factory )
			{
				this.factory = factory;
			}

			protected override IServiceProvider CreateItem( AutoData parameter ) => 
				new ServiceProviderContainerFactory( new Func<IServiceProvider>( new Activation.IoC.ServiceProviderFactory( () => factory( parameter ) ).Create ) ).Create();
		}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		public AssemblyProvider( params Type[] others ) : base( others.Append( typeof(AssemblySourceBase) ).Fixed(), DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}
}
