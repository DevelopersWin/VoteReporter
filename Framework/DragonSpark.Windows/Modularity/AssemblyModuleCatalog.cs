using DragonSpark.Modularity;
using DragonSpark.Setup.Registration;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Windows.Modularity
{
	[Register.Mapped]
	public class AssemblyModuleCatalog : DragonSpark.Modularity.AssemblyModuleCatalog
	{
		public AssemblyModuleCatalog( ImmutableArray<Assembly> assemblies, IModuleInfoBuilder builder ) : base( assemblies, builder ) {}
	}
}