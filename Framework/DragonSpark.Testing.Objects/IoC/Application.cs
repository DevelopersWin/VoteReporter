using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Objects.IoC
{
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		readonly static Func<IServiceProvider> Factory = new DragonSpark.Setup.AssemblyBasedServiceProviderFactory( Items<Assembly>.Default ).Create;

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		protected UnityContainerFactory() : base( Factory() ) {}

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		readonly static Assembly[] Assemblies = AssemblyProvider.Instance.Create();

		public AutoDataAttribute() : this( DefaultApplicationSource ) {}

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> applicationSource ) : this( Assemblies, applicationSource ) {}

		protected AutoDataAttribute( IEnumerable<Assembly> assemblies, Func<IServiceProvider, IApplication> applicationSource ) : base( new Factory( assemblies ), applicationSource ) {}

		class Factory : FactoryBase<AutoData, IServiceProvider>
		{
			readonly ImmutableArray<Assembly> assemblySource;

			public Factory( IEnumerable<Assembly> assemblySource )
			{
				this.assemblySource = assemblySource.ToImmutableArray();
			}

			public override IServiceProvider Create( AutoData parameter ) => new Activation.IoC.ServiceProviderFactory( Cache.Instance.Get( parameter.Method.DeclaringType ).Get( assemblySource ) ).Create();

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
		readonly static Assembly ApplicationAssembly = DomainApplicationAssemblyLocator.Instance.Get( AppDomain.CurrentDomain );

		public static AssemblyProvider Instance { get; } = new AssemblyProvider();
		public AssemblyProvider( params Type[] others ) : base( others.Append( typeof(AssemblySourceBase) ).Fixed(), ApplicationAssembly ) {}
	}
}
