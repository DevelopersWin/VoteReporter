using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class FileSystemAssemblySource : DelegatedCachedSource<IEnumerable<Assembly>>
	{
		public static ISource<IEnumerable<Assembly>> Instance { get; } = new FileSystemAssemblySource();
		FileSystemAssemblySource() : base( Create ) {}

		static IEnumerable<Assembly> Create() =>
			AllClasses.FromAssembliesInBasePath( includeUnityAssemblies: true )
					  .Where( x => x.Namespace != null )
					  .GroupBy( t => t.Assembly )
					  .Select( g => g.Key )
					  .ToArray();
	}
}