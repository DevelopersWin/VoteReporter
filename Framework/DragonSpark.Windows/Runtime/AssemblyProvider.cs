using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using System.Linq;

namespace DragonSpark.Windows.Runtime
{
	public class AssemblyProvider : AggregateAssemblyFactory
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();
		AssemblyProvider() : base( FileSystemAssemblySource.Instance.Create().ToArray().Self, ApplicationAssemblyFilter.Instance.Get ) {}
	}
}