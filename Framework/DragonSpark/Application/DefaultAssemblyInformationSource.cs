using System.Reflection;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;

namespace DragonSpark.Application
{
	public sealed class DefaultAssemblyInformationSource : FixedFactory<Assembly, AssemblyInformation>
	{
		public static ISource<AssemblyInformation> Default { get; } = /*new Scope<AssemblyInformation>( Factory.Global( () => .Get() ) ).ScopedWithDefault()*/new DefaultAssemblyInformationSource();
		DefaultAssemblyInformationSource() : base( AssemblyInformationSource.Default.Get, ApplicationAssembly.Default.Get ) {}
	}
}