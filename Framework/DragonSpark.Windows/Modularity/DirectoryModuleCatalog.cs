using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Modularity;
using DragonSpark.Setup;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Windows.Runtime;

namespace DragonSpark.Windows.Modularity
{
	public class DirectoryModuleCatalog : AssemblyModuleCatalog
	{
		readonly IValidatedParameterizedSource<LoadRemoteModuleInfoParameter, ModuleInfo[]> factory;

		public DirectoryModuleCatalog() : this( DomainAssemblySource.Instance.Get( AppDomain.CurrentDomain ).ToImmutableArray(), ModuleInfoBuilder.Instance, LoadRemoteModuleInfoFactory.Instance ) {}

		public DirectoryModuleCatalog( ImmutableArray<Assembly> assemblies, IModuleInfoBuilder builder, IValidatedParameterizedSource<LoadRemoteModuleInfoParameter, ModuleInfo[]> factory ) : base( assemblies, builder )
		{
			this.factory = factory;
			ModulePath = ".";
		}

		public string ModulePath { get; set; }

		protected override IEnumerable<ModuleInfo> GetModuleInfos( ImmutableArray<Assembly> assemblies )
		{
			var parameter = new LoadRemoteModuleInfoParameter( assemblies.Select( assembly => assembly.Location ), ModulePath );
			var result = factory.Get( parameter );
			return result;
		}
	}

	public class LoadRemoteModuleInfoFactory : ValidatedParameterizedSourceBase<LoadRemoteModuleInfoParameter, ModuleInfo[]>
	{
		public static LoadRemoteModuleInfoFactory Instance { get; } = new LoadRemoteModuleInfoFactory();

		readonly IValidatedParameterizedSource<LoadRemoteModuleInfoParameter, IModuleInfoProvider> factory;

		public LoadRemoteModuleInfoFactory() : this( RemoteModuleInfoProviderFactory.Instance )
		{}

		public LoadRemoteModuleInfoFactory( IValidatedParameterizedSource<LoadRemoteModuleInfoParameter, IModuleInfoProvider> factory )
		{
			this.factory = factory;
		}

		public override ModuleInfo[] Get( LoadRemoteModuleInfoParameter parameter )
		{
			using ( var loader = factory.Get( parameter ) )
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

	public class RemoteModuleInfoProviderFactory : ValidatedParameterizedSourceBase<LoadRemoteModuleInfoParameter, IModuleInfoProvider>
	{
		public static RemoteModuleInfoProviderFactory Instance { get; } = new RemoteModuleInfoProviderFactory();

		readonly IModuleInfoBuilder builder;
		readonly IValidatedParameterizedSource<AppDomain, AppDomain> factory;

		public RemoteModuleInfoProviderFactory() : this( ModuleInfoBuilder.Instance, ChildDomainFactory.Instance )
		{}

		public RemoteModuleInfoProviderFactory( IModuleInfoBuilder builder, IValidatedParameterizedSource<AppDomain, AppDomain> factory )
		{
			this.builder = builder;
			this.factory = factory;
		}

		public override IModuleInfoProvider Get( LoadRemoteModuleInfoParameter parameter )
		{
			var loaded = parameter.Locations.ToArray();
			var child = factory.Get( AppDomain.CurrentDomain );
			var result = new RemotingModuleInfoProvider<DirectoryModuleInfoProvider>( child, builder, loaded, parameter.Path );
			return result;
		}
	}

	public class ChildDomainFactory : ValidatedParameterizedSourceBase<AppDomain, AppDomain>
	{
		public static ChildDomainFactory Instance { get; } = new ChildDomainFactory();

		readonly string name;

		public ChildDomainFactory( string name = "DiscoveryRegion" )
		{
			this.name = name;
		}

		public override AppDomain Get( AppDomain parameter )
		{
			var evidence = new Evidence(parameter.Evidence);
			var setup = parameter.SetupInformation;
			var result = AppDomain.CreateDomain( name, evidence, setup );
			return result;
		}
	}
}