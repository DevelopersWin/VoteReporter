using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Objects.IoC
{
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		readonly static Func<IServiceProvider> Factory = new DragonSpark.Setup.AssemblyBasedServiceProviderFactory( Items<Assembly>.Default ).Create;

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		UnityContainerFactory() : base( Factory() ) {}

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		readonly static Func<Assembly[]> AssemblySource = AssemblyProvider.Instance.ToDelegate();

		public AutoDataAttribute() : this( DefaultApplicationSource ) {}

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> applicationSource ) : this( AssemblySource, applicationSource ) {}

		protected AutoDataAttribute( Func<Assembly[]> assemblySource, Func<IServiceProvider, IApplication> applicationSource ) : base( new Factory( assemblySource ), applicationSource ) {}

		class Factory : FactoryBase<AutoData, IServiceProvider>
		{
			readonly Func<Assembly[]> assemblySource;

			public Factory( Func<Assembly[]> assemblySource )
			{
				this.assemblySource = assemblySource;
			}

			public override IServiceProvider Create( AutoData parameter ) => new Activation.IoC.ServiceProviderFactory( Cache.Instance.Get( parameter.Method.DeclaringType ).Get( assemblySource().ToImmutableArray() ) ).Create();

			sealed class Cache : ActivatedCache<Cache.ProviderCache>
			{
				public new static Cache Instance { get; } = new Cache();

				public class ProviderCache : ArgumentCache<ImmutableArray<Assembly>, IServiceProvider>
				{
					public ProviderCache() : base( assemblies => new AssemblyBasedServiceProviderFactory( assemblies.ToArray() ).Create() ) {}
				}
			}
		}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		readonly static Assembly ApplicationAssembly = DomainApplicationAssemblyLocator.Instance.Create();

		public static AssemblyProvider Instance { get; } = new AssemblyProvider();
		public AssemblyProvider( params Type[] others ) : base( others.Append( typeof(AssemblySourceBase) ).Fixed(), ApplicationAssembly ) {}
	}
}
