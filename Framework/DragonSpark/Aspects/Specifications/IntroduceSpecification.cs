using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceSpecification : IntroduceGenericInterfaceAspectBase
	{
		readonly static Func<Type, Func<object, object>> Factory = new ImplementationCache( typeof(ISpecificationAdapter) ).ToCache().ToDelegate();

		public IntroduceSpecification() : this( typeof(DefaultSpecificationImplementation<>) ) {}
		public IntroduceSpecification( Type implementationType ) : this( Factory( implementationType ) ) {}
		public IntroduceSpecification( Func<object, object> factory ) : base( SpecificationTypeDefinition.Default, factory ) {}
	}
}