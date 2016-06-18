using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using PostSharp.Aspects.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DragonSpark.TypeSystem
{
	public class ApplicationAssemblyFilter : TransformerBase<Assembly[]>
	{
		public static ApplicationAssemblyFilter Instance { get; } = new ApplicationAssemblyFilter();

		readonly ISpecification<Assembly> specification;
		
		static string[] Determine( IEnumerable<Assembly> coreAssemblies ) => coreAssemblies.Alive().Append( typeof(ApplicationAssemblyFilter).Assembly() ).Distinct().Select( assembly => assembly.GetRootNamespace() ).ToArray();

		public ApplicationAssemblyFilter( [PostSharp.Patterns.Contracts.Required]params Assembly[] coreAssemblies ) : this( Determine( coreAssemblies ) ) {}

		public ApplicationAssemblyFilter( string[] namespaces ) : this( new ApplicationAssemblySpecification( namespaces ) ) {}

		public ApplicationAssemblyFilter( ISpecification<Assembly> specification )
		{
			this.specification = specification;
		}

		public override Assembly[] Create( Assembly[] parameter ) => parameter.Where( specification.IsSatisfiedBy ).Prioritize().ToArray();
	}

	public class ApplicationTypeSpecification : GuardedSpecificationBase<Type>
	{
		public static ApplicationTypeSpecification Instance { get; } = new ApplicationTypeSpecification();

		ApplicationTypeSpecification() {}

		[Freeze]
		public override bool IsSatisfiedBy( Type parameter ) => CanInstantiateSpecification.Instance.IsSatisfiedBy( parameter ) && !typeof(MethodBinding).Adapt().IsAssignableFrom( parameter ) && !parameter.Adapt().IsDefined<CompilerGeneratedAttribute>();
	}

	public class ApplicationAssemblySpecification : GuardedSpecificationBase<Assembly>
	{
		public static ApplicationAssemblySpecification Instance { get; } = new ApplicationAssemblySpecification();

		readonly string[] rootNamespaces;

		public ApplicationAssemblySpecification( [PostSharp.Patterns.Contracts.Required] params string[] rootNamespaces )
		{
			this.rootNamespaces = rootNamespaces;
		}

		public override bool IsSatisfiedBy( Assembly parameter ) => parameter.Has<RegistrationAttribute>() || rootNamespaces.Any( parameter.GetName().Name.StartsWith );
	}
}