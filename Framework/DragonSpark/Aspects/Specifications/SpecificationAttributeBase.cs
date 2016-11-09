using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISpecification) )]
	[ProvideAspectRole( KnownRoles.ParameterValidation ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public abstract class SpecificationAttributeBase : InvocationAspectBase, ISpecification
	{
		protected SpecificationAttributeBase( Func<object, IAspect> factory ) : base( factory, Definition.Default ) {}
		protected SpecificationAttributeBase( ISpecification specification ) : base( specification.Get ) {}

		protected sealed class Factory<T> : TypedParameterAspectFactory<ISpecification, T> where T : SpecificationAttributeBase
		{
			public static Factory<T> Default { get; } = new Factory<T>();
			Factory() : base( Source.Default.Get ) {}
		}
	}
}