using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[ProvideAspectRole( KnownRoles.ParameterValidation ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public abstract class SpecificationAttributeBase : InvocationAspectBase, ISpecification
	{
		protected SpecificationAttributeBase( Func<object, IAspect> factory ) : base( factory, Support.Default ) {}
		protected SpecificationAttributeBase( ISpecification specification ) : base( specification ) {}

		protected sealed class Factory<T> : TypedAspectFactory<ISpecification, T> where T :  SpecificationAttributeBase
		{
			public static Factory<T> Default { get; } = new Factory<T>();
			Factory() : base( Source.Default.Get ) {}
		}
	}
}