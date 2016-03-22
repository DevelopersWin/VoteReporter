using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class ApplicationAssemblyFilter : TransformerBase<Assembly[]>
	{
		readonly ISpecification<Assembly> specification;
		
		static string[] Determine( IEnumerable<Assembly> coreAssemblies ) => coreAssemblies.NotNull().Append( typeof(ApplicationAssemblyFilter).Assembly() ).Distinct().Select( assembly => assembly.GetRootNamespace() ).ToArray();

		public ApplicationAssemblyFilter( [PostSharp.Patterns.Contracts.Required]params Assembly[] coreAssemblies ) : this( Determine( coreAssemblies ) ) {}

		public ApplicationAssemblyFilter( string[] namespaces ) : this( new ApplicationAssemblySpecification( namespaces ) ) {}

		public ApplicationAssemblyFilter( ISpecification<Assembly> specification )
		{
			this.specification = specification;
		}

		protected override Assembly[] CreateItem( Assembly[] parameter ) => parameter.Where( specification.IsSatisfiedBy ).Prioritize().ToArray();
	}

	public class ApplicationAssemblySpecification : SpecificationBase<Assembly>
	{
		public static ApplicationAssemblySpecification Instance { get; } = new ApplicationAssemblySpecification();

		readonly string[] rootNamespaces;

		public ApplicationAssemblySpecification( [PostSharp.Patterns.Contracts.Required] params string[] rootNamespaces )
		{
			this.rootNamespaces = rootNamespaces;
		}

		protected override bool Verify( Assembly parameter ) => parameter.Has<RegistrationAttribute>() || rootNamespaces.Any( parameter.GetName().Name.StartsWith );
	}
}