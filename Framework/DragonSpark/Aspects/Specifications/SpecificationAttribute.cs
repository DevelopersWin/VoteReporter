using System;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISpecification), OverrideAction = InterfaceOverrideAction.Ignore )]
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