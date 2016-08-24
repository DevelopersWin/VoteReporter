using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Delegates;
using DragonSpark.Specifications;

namespace DragonSpark.Application
{
	public class ApplicationAssemblySpecification : SpecificationBase<Assembly>
	{
		public static ApplicationAssemblySpecification Default { get; } = new ApplicationAssemblySpecification();
		ApplicationAssemblySpecification() {}

		/*readonly ImmutableArray<string> rootNamespaces;

		public ApplicationAssemblySpecification( IEnumerable<Assembly> assemblies ) : this( Determine( assemblies ) ) {}

		ApplicationAssemblySpecification( ImmutableArray<string> rootNamespaces )
		{
			this.rootNamespaces = rootNamespaces;
		}

		static ImmutableArray<string> Determine( IEnumerable<Assembly> coreAssemblies ) => coreAssemblies.Append( typeof(ApplicationAssemblyFilter).Assembly() ).Distinct().Select( assembly => assembly.GetRootNamespace() ).ToImmutableArray();*/

		public override bool IsSatisfiedBy( Assembly parameter ) => parameter.Has<RegistrationAttribute>()/* || rootNamespaces.Any( parameter.GetName().Name.StartsWith )*/;
	}
}