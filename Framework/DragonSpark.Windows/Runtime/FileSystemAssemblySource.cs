using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class FileSystemAssemblySource : AssemblySourceBase
	{
		public static FileSystemAssemblySource Instance { get; } = new FileSystemAssemblySource();
		FileSystemAssemblySource() : base( Create() ) {}

		new static IEnumerable<Assembly> Create()
		{
			var types = AllClasses.FromAssembliesInBasePath( includeUnityAssemblies: true );
			var result = types
				.Where( x => x.Namespace != null )
				.GroupBy( t => t.Assembly )
				.Select( g => g.Key );
			return result;
		}
	}
}