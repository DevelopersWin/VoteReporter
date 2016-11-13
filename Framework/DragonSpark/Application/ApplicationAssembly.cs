using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Application
{
	public sealed class ApplicationAssembly : SuppliedSource<IEnumerable<Assembly>, Assembly>
	{
		public static ApplicationAssembly Default { get; } = new ApplicationAssembly();
		ApplicationAssembly() : base( ApplicationAssemblyLocator.Default.Get, ApplicationAssemblies.Default.GetEnumerable ) {}
	}
}