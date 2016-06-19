using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using System;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class AssemblyProvider : AggregateAssemblyFactory
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		public AssemblyProvider() : this( FileSystemAssemblySource.Instance.ToDelegate() ) {}

		public AssemblyProvider( Func<Assembly[]> source ) : base( source, ApplicationAssemblyFilter.Instance.ToDelegate() ) {}
	}
}