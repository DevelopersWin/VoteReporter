using DragonSpark.Extensions;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class ApplicationAssemblyFilter : DragonSpark.TypeSystem.ApplicationAssemblyFilter
	{
		public static ApplicationAssemblyFilter Instance { get; } = new ApplicationAssemblyFilter();

		public ApplicationAssemblyFilter() : base( DetermineCoreAssemblies() ) {}

		static Assembly[] DetermineCoreAssemblies()
		{
			var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
			var result = assembly.Append( typeof(ApplicationAssemblyFilter).Assembly ).Fixed();
			return result;
		}
	}
}