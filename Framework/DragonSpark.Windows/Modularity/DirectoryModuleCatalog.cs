using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Modularity;
using DragonSpark.Windows.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;

namespace DragonSpark.Windows.Modularity
{
	public class DirectoryModuleCatalog : AssemblyModuleCatalog
	{
		readonly IFactory<LoadRemoteModuleInfoParameter, ModuleInfo[]> factory;

		public DirectoryModuleCatalog() : this( AssemblyProvider.Instance.Create(), ModuleInfoBuilder.Instance, LoadRemoteModuleInfoFactory.Instance ) {}

		public DirectoryModuleCatalog( Assembly[] assemblies, IModuleInfoBuilder builder, IFactory<LoadRemoteModuleInfoParameter, ModuleInfo[]> factory ) : base( assemblies, builder )
		{
			this.factory = factory;
			ModulePath = ".";
		}

		public string ModulePath { get; set; }

		protected override IEnumerable<ModuleInfo> GetModuleInfos( IEnumerable<Assembly> assemblies )
		{
			var parameter = new LoadRemoteModuleInfoParameter( assemblies.Select( assembly => assembly.Location ), ModulePath );
			var result = factory.Create( parameter );
			return result;
		}
	}

	public class LoadRemoteModuleInfoFactory : FactoryWithSpecificationBase<LoadRemoteModuleInfoParameter, ModuleInfo[]>
	{
		public static LoadRemoteModuleInfoFactory Instance { get; } = new LoadRemoteModuleInfoFactory();

		readonly IFactory<LoadRemoteModuleInfoParameter, IModuleInfoProvider> factory;

		public LoadRemoteModuleInfoFactory() : this( RemoteModuleInfoProviderFactory.Instance )
		{}

		public LoadRemoteModuleInfoFactory( IFactory<LoadRemoteModuleInfoParameter, IModuleInfoProvider> factory )
		{
			this.factory = factory;
		}

		public override ModuleInfo[] Create( LoadRemoteModuleInfoParameter parameter )
		{
			using ( var loader = factory.Create( parameter ) )
			{
				var result = loader.GetModuleInfos().Fixed();
				return result;
			}
		}
	}

	public class LoadRemoteModuleInfoParameter
	{
		public LoadRemoteModuleInfoParameter( IEnumerable<string> assemblyLocations, string path )
		{
			Locations = assemblyLocations.Fixed();
			Path = path;
		}

		public string[] Locations { get; }
		public string Path { get; }
	}

	public class RemoteModuleInfoProviderFactory : FactoryWithSpecificationBase<LoadRemoteModuleInfoParameter, IModuleInfoProvider>
	{
		public static RemoteModuleInfoProviderFactory Instance { get; } = new RemoteModuleInfoProviderFactory();

		readonly IModuleInfoBuilder builder;
		readonly IFactory<AppDomain, AppDomain> factory;

		public RemoteModuleInfoProviderFactory() : this( ModuleInfoBuilder.Instance, ChildDomainFactory.Instance )
		{}

		public RemoteModuleInfoProviderFactory( IModuleInfoBuilder builder, IFactory<AppDomain, AppDomain> factory )
		{
			this.builder = builder;
			this.factory = factory;
		}

		public override IModuleInfoProvider Create( LoadRemoteModuleInfoParameter parameter )
		{
			var loaded = parameter.Locations.ToArray();
			var child = factory.Create( AppDomain.CurrentDomain );
			var result = new RemotingModuleInfoProvider<DirectoryModuleInfoProvider>( child, builder, loaded, parameter.Path );
			return result;
		}
	}

	public class ChildDomainFactory : FactoryWithSpecificationBase<AppDomain, AppDomain>
	{
		public static ChildDomainFactory Instance { get; } = new ChildDomainFactory();

		readonly string name;

		public ChildDomainFactory( string name = "DiscoveryRegion" )
		{
			this.name = name;
		}

		public override AppDomain Create( AppDomain parameter )
		{
			var evidence = new Evidence(parameter.Evidence);
			var setup = parameter.SetupInformation;
			var result = AppDomain.CreateDomain( name, evidence, setup );
			return result;
		}
	}
}