using DragonSpark.TypeSystem;
using System;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class AssemblyProvider : AggregateAssemblyFactory
	{
		readonly static Func<Assembly[], Assembly[]> Filter = ApplicationAssemblyFilter.Instance.Create;

		public static AssemblyProvider Instance { get; } = new AssemblyProvider();
		AssemblyProvider() : this( FileSystemAssemblySource.Instance.Create ) {}

		public AssemblyProvider( Func<Assembly[]> source ) : base( source, Filter ) {}
	}
}