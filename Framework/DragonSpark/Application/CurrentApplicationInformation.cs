using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System.Reflection;

namespace DragonSpark.Application
{
	public sealed class CurrentApplicationInformation : SuppliedSource<Assembly, AssemblyInformation>
	{
		public static ISource<AssemblyInformation> Default { get; } = new CurrentApplicationInformation();
		CurrentApplicationInformation() : base( AssemblyInformationStore.Default.Get, ApplicationAssembly.Default.Get ) {}
	}
}