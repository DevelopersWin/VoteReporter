using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISource<ISpecificationAdapter>) )]
	[ProvideAspectRole( KnownRoles.ParameterValidation ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public abstract class SpecificationAspectBase : InvocationAspectBase, ISource<ISpecificationAdapter>
	{
		readonly ISpecificationAdapter specification;
		protected SpecificationAspectBase( Func<object, IAspect> factory ) : base( factory, Definition.Default ) {}
		protected SpecificationAspectBase( ISpecificationAdapter specification )
		{
			this.specification = specification;
		}

		protected sealed class Factory<T> : TypedParameterAspectFactory<ISpecificationAdapter, T> where T : SpecificationAspectBase
		{
			public static Factory<T> Default { get; } = new Factory<T>();
			Factory() : base( Source.Default.Get ) {}
		}

		public ISpecificationAdapter Get() => specification;
		// object ISource.Get() => Get();
	}
}