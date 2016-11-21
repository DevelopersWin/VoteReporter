using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System.Reflection;

namespace DragonSpark.Application
{
	public sealed class AssemblyInformationContext : SuppliedSource<Assembly, AssemblyInformation>
	{
		public static ISource<AssemblyInformation> Default { get; } = new AssemblyInformationContext();
		AssemblyInformationContext() : base( AssemblyInformationStore.Default.Get, ApplicationAssembly.Default.Get ) {}
	}
}