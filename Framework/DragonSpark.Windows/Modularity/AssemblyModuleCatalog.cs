using System.Reflection;
using DragonSpark.Modularity;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;

namespace DragonSpark.Windows.Modularity
{
	[Register.Mapped]
	public class AssemblyModuleCatalog : DragonSpark.Modularity.AssemblyModuleCatalog
	{
		public AssemblyModuleCatalog( Assembly[] assemblies, IModuleInfoBuilder builder ) : base( assemblies, builder ) {}
	}
}