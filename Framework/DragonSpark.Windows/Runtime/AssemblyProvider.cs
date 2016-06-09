using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class AssemblyProvider : AggregateAssemblyFactory
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		public AssemblyProvider() : this( FileSystemAssemblySource.Instance ) {}

		public AssemblyProvider( IFactory<Assembly[]> source ) : base( source, ImmutableArray.Create<ITransformer<Assembly[]>>( ApplicationAssemblyFilter.Instance ) ) {}
	}
}