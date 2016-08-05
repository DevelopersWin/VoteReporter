using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using PostSharp.Aspects.Internals;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DragonSpark.TypeSystem
{
	public class ApplicationAssemblyFilter : TransformerBase<IEnumerable<Assembly>>
	{
		readonly Func<Assembly, bool> specification;

		public ApplicationAssemblyFilter( params Assembly[] assemblies ) : this( new ApplicationAssemblySpecification( assemblies ).IsSatisfiedBy ) {}

		ApplicationAssemblyFilter( Func<Assembly, bool> specification )
		{
			this.specification = specification;
		}

		public override IEnumerable<Assembly> Get( IEnumerable<Assembly> parameter ) => parameter.Where( specification );
	}

	public class ApplicationTypeSpecification : GuardedSpecificationBase<Type>
	{
		public static ISpecification<Type> Instance { get; } = new ApplicationTypeSpecification().Cached();
		ApplicationTypeSpecification() {}

		public override bool IsSatisfiedBy( Type parameter ) => Defaults.ActivateSpecification.IsSatisfiedBy( parameter ) && !typeof(MethodBinding).Adapt().IsAssignableFrom( parameter ) && !parameter.Has<CompilerGeneratedAttribute>();
	}

	public class ApplicationAssemblySpecification : GuardedSpecificationBase<Assembly>
	{
		readonly ImmutableArray<string> rootNamespaces;

		public ApplicationAssemblySpecification( IEnumerable<Assembly> assemblies ) : this( Determine( assemblies ) ) {}

		ApplicationAssemblySpecification( ImmutableArray<string> rootNamespaces )
		{
			this.rootNamespaces = rootNamespaces;
		}

		static ImmutableArray<string> Determine( IEnumerable<Assembly> coreAssemblies ) => coreAssemblies.Append( typeof(ApplicationAssemblyFilter).Assembly() ).Distinct().Select( assembly => assembly.GetRootNamespace() ).ToImmutableArray();

		public override bool IsSatisfiedBy( Assembly parameter ) => parameter.Has<RegistrationAttribute>() || rootNamespaces.Any( parameter.GetName().Name.StartsWith );
	}
}