using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System;
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

		public UnityContainerFactory() : base( new DragonSpark.Setup.ServiceProviderFactory( Default<Assembly>.Items ).Create ) {}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : this( provider => new Application( provider ) ) {}

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> applicationSource ) : this( AssemblyProvider.Instance.Create, applicationSource ) {}

		protected AutoDataAttribute( Func<Assembly[]> assemblySource, Func<IServiceProvider, IApplication> applicationSource ) 
			: base( Providers.From( data => new Activation.IoC.ServiceProviderFactory( () => new Cache( assemblySource() ).Create( data ) ).Create(), applicationSource ) ) {}

		class Cache : CacheFactoryBase
		{
			public Cache( Assembly[] assemblies ) : base( data => assemblies, new ServiceProviderFactory( assemblies ).Create ) {}
		}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		public AssemblyProvider( params Type[] others ) : base( others.Append( typeof(AssemblySourceBase) ).Fixed(), DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}
}
