using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceSpecification : IntroduceGenericInterfaceAspectBase
	{
		readonly static Func<object, object> Factory = new GenericAdapterFactory( typeof(ISpecificationAdapter), typeof(DefaultSpecificationImplementation<>) ).Get;

		public IntroduceSpecification() : this( Factory ) {}
		public IntroduceSpecification( Type implementationType ) : this( new GenericAdapterFactory( typeof(ISpecificationAdapter), implementationType ).Get ) {}
		public IntroduceSpecification( Func<object, object> factory ) : base( GenericSpecificationTypeDefinition.Default.ReferencedType, Factory ) {}
	}
}