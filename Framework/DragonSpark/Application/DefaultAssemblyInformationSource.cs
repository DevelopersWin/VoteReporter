using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System.Reflection;

namespace DragonSpark.Application
{
	public sealed class DefaultAssemblyInformationSource : FixedFactory<Assembly, AssemblyInformation>
	{
		public static ISource<AssemblyInformation> Default { get; } = new DefaultAssemblyInformationSource();
		DefaultAssemblyInformationSource() : base( AssemblyInformationSource.Default.Get, ApplicationAssembly.Default.Get ) {}
	}
}