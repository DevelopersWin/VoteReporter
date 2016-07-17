using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using PostSharp;
using PostSharp.Extensibility;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using IApplication = DragonSpark.Testing.Framework.Setup.IApplication;

namespace DragonSpark.Testing.Objects.IoC
{
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		readonly static Func<IServiceProvider> Factory = () => ServiceProvider.From( Items<Assembly>.Default );

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		protected UnityContainerFactory() : base( Factory() ) {}

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		readonly static ImmutableArray<Assembly> Assemblies = asdf();

		static ImmutableArray<Assembly> asdf()
		{
			try
{
				return AssemblyProvider.Instance.Create();
}
catch ( Exception e )
{
	MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {e}", null, null, null ));
	throw;
}

			
		}

		public AutoDataAttribute() : this( DefaultApplicationSource ) {}

		protected AutoDataAttribute( Func<IApplication> applicationSource ) : this( Assemblies, applicationSource ) {}

		protected AutoDataAttribute( ImmutableArray<Assembly> assemblies, Func<IApplication> applicationSource ) : base( new Factory( assemblies ), applicationSource ) {}

		class Factory : FactoryBase<AutoData, IServiceProvider>
		{
			readonly ImmutableArray<Assembly> assemblySource;

			public Factory( ImmutableArray<Assembly> assemblySource )
			{
				this.assemblySource = assemblySource;
			}

			public override IServiceProvider Create( AutoData parameter ) => new Activation.IoC.ServiceProviderFactory( Cache.Instance.Get( parameter.Method.DeclaringType ).Get( assemblySource ) ).Create();

			sealed class Cache : ActivatedCache<Cache.ProviderCache>
			{
				public new static Cache Instance { get; } = new Cache();

				public class ProviderCache : ArgumentCache<ImmutableArray<Assembly>, IServiceProvider>
				{
					public ProviderCache() : base( assemblies => ServiceProvider.From( assemblies.ToArray() ) ) {}
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
