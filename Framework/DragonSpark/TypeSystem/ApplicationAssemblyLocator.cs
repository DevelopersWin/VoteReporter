using DragonSpark.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class ApplicationAssemblyLocator : ParameterizedSourceBase<IEnumerable<Assembly>, Assembly>
	{
		public static ApplicationAssemblyLocator Instance { get; } = new ApplicationAssemblyLocator();
		ApplicationAssemblyLocator() {}

		public override Assembly Get( IEnumerable<Assembly> assemblies ) => assemblies.SingleOrDefault( assembly => assembly.IsDefined( typeof(ApplicationAttribute) ) );
	}
}