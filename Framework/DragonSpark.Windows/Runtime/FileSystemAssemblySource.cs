using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class FileSystemAssemblySource : FixedDeferedSource<ImmutableArray<Assembly>>
	{
		public static ISource<ImmutableArray<Assembly>> Default { get; } = new FileSystemAssemblySource();
		FileSystemAssemblySource() : this( AppDomain.CurrentDomain ) {}

		public FileSystemAssemblySource( AppDomain domain ) : base( new FixedFactory<AppDomain, ImmutableArray<Assembly>>( DomainAssemblySource.Default.Get, domain ).Get ) {}

		//static IEnumerable<Assembly> Create( AppDomain domain ) => DomainApplicationAssemblies.Default.Get( domain );
			/*AllClasses.FromAssembliesInBasePath( includeUnityAssemblies: true )
					  .Where( x => x.Namespace != null )
					  .GroupBy( t => t.Assembly )
					  .Select( g => g.Key )
					  .ToArray();*/
	}
}