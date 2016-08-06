using DragonSpark.Extensions;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class ApplicationAssemblyFilter : DragonSpark.TypeSystem.ApplicationAssemblyFilter
	{
		public static ApplicationAssemblyFilter Instance { get; } = new ApplicationAssemblyFilter();
		ApplicationAssemblyFilter() : base( DetermineCoreAssemblies() ) {}

		static ImmutableArray<Assembly> DetermineCoreAssemblies()
		{
			var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
			var result = assembly.Append( typeof(ApplicationAssemblyFilter).Assembly ).ToImmutableArray();
			return result;
		}
	}
}