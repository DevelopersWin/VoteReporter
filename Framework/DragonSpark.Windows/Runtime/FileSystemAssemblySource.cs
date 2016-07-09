using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class FileSystemAssemblySource : AssemblySourceBase
	{
		public static FileSystemAssemblySource Instance { get; } = new FileSystemAssemblySource();

		protected override Assembly[] Cache()
		{
			var fromAssembliesInBasePath = AllClasses.FromAssembliesInBasePath( includeUnityAssemblies: true );
			var result = fromAssembliesInBasePath
				.Where( x => x.Namespace != null )
				.GroupBy( type => type.Assembly )
				.Select( types => types.Key ).ToArray();
			return result;
		}
	}
}