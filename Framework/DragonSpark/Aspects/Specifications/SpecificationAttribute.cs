using System;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISpecification), OverrideAction = InterfaceOverrideAction.Ignore )]
	[LinesOfCodeAvoided( 1 ), ProvideAspectRole( KnownRoles.ParameterValidation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public sealed class SpecificationAttribute : SpecificationAttributeBase
	{
		readonly static Func<Type, ISpecification> Source = SpecificationSource.Default.Get;

		readonly Type specificationType;
		readonly Func<Type, ISpecification> source;

		public SpecificationAttribute( Type specificationType ) : this( specificationType, Source ) {}
		SpecificationAttribute( Type specificationType, Func<Type, ISpecification> source )
		{
			this.specificationType = specificationType;
			this.source = source;
		}

		protected override ISpecification DetermineSpecification() => source( specificationType );
	}
}