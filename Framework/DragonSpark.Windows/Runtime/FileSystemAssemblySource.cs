using DragonSpark.Activation;
using System;
using System.Collections.Immutable;
using System.Reflection;
using DragonSpark.Activation.Sources;

namespace DragonSpark.Windows.Runtime
{
	public class FileSystemAssemblySource : FixedDeferedSource<ImmutableArray<Assembly>>
	{
		public static ISource<ImmutableArray<Assembly>> Instance { get; } = new FileSystemAssemblySource();
		FileSystemAssemblySource() : this( AppDomain.CurrentDomain ) {}

		public FileSystemAssemblySource( AppDomain domain ) : base( new FixedFactory<AppDomain, ImmutableArray<Assembly>>( DomainAssemblySource.Instance.Get, domain ).Get ) {}

		//static IEnumerable<Assembly> Create( AppDomain domain ) => DomainApplicationAssemblies.Instance.Get( domain );
			/*AllClasses.FromAssembliesInBasePath( includeUnityAssemblies: true )
					  .Where( x => x.Namespace != null )
					  .GroupBy( t => t.Assembly )
					  .Select( g => g.Key )
					  .ToArray();*/
	}
}