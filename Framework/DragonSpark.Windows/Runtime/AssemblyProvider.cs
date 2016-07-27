using DragonSpark.TypeSystem;

namespace DragonSpark.Windows.Runtime
{
	public class AssemblyProvider : AssemblySourceBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();
		AssemblyProvider() : base( ApplicationAssemblyFilter.Instance.Get( FileSystemAssemblySource.Instance.Get() ) ) {}
	}
}