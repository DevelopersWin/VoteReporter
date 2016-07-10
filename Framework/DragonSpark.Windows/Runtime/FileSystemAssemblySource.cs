using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class FileSystemAssemblySource : AssemblyStoreBase
	{
		public static FileSystemAssemblySource Instance { get; } = new FileSystemAssemblySource();
		FileSystemAssemblySource() : base( Factory.Instance.Create() ) {}

		class Factory : FactoryBase<Assembly[]>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override Assembly[] Create()
			{
				var types = AllClasses.FromAssembliesInBasePath( includeUnityAssemblies: true );
				var result = types
					.Where( x => x.Namespace != null )
					.GroupBy( t => t.Assembly )
					.Select( g => g.Key ).ToArray();
				return result;
			}
		}
	}
}