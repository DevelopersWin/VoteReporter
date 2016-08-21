using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DragonSpark.TypeSystem
{
	public class ApplicationAssemblyFilter : TransformerBase<IEnumerable<Assembly>>
	{
		readonly Func<Assembly, bool> specification;

		/*public ApplicationAssemblyFilter() : this( ApplicationAssemblies.Default.Get() ) {}*/

		public static ApplicationAssemblyFilter Default { get; } = new ApplicationAssemblyFilter();
		ApplicationAssemblyFilter( /*ImmutableArray<Assembly> assemblies*/ ) : this( ApplicationAssemblySpecification.Default.IsSatisfiedBy ) {}

		ApplicationAssemblyFilter( Func<Assembly, bool> specification )
		{
			this.specification = specification;
		}

		public override IEnumerable<Assembly> Get( IEnumerable<Assembly> parameter ) => parameter.Where( specification );
	}

	public class ApplicationTypeSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new ApplicationTypeSpecification().Cached();
		ApplicationTypeSpecification() {}

		public override bool IsSatisfiedBy( Type parameter ) => Defaults.ActivateSpecification.IsSatisfiedBy( parameter ) && !typeof(MethodBinding).Adapt().IsAssignableFrom( parameter ) && !parameter.Has<CompilerGeneratedAttribute>();
	}

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