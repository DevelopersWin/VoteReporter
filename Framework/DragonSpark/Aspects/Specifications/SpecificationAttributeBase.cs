using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Specifications
{
	[ProvideAspectRole( "Specification" ), LinesOfCodeAvoided( 1 )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public abstract class SpecificationAttributeBase : ApplyAspectBase, ISpecification
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = new AspectInstances( SpecificationProfile.Default ).ToSourceDelegate();
		readonly static Func<Type, bool> DefaultSpecification = new Specification( Defaults.Specification.DeclaringType ).ToSpecificationDelegate();

		protected SpecificationAttributeBase() : base( DefaultSpecification, DefaultSource ) {}

		ISpecification Specification { get; set; }
		public override void RuntimeInitializeInstance() => Specification = DetermineSpecification();
		protected abstract ISpecification DetermineSpecification();
		bool ISpecification.IsSatisfiedBy( object parameter ) => Specification.IsSatisfiedBy( parameter );
	}
}