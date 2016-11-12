using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class IntroduceGeneralizedSpecification : IntroduceInterfaceAspectBase
	{
		readonly static Func<Type, Func<object, object>> Factory = new ImplementationCache( SpecificationTypeDefinition.Default.ReferencedType ).ToCache().ToDelegate();
		
		public IntroduceGeneralizedSpecification() : this( typeof(SpecificationAdapter<>) ) {}

		public IntroduceGeneralizedSpecification( Type implementationType ) : this( Factory( implementationType ) ) {}

		[UsedImplicitly]
		public IntroduceGeneralizedSpecification( Func<object, object> factory ) : base( GeneralizedSpecificationTypeDefinition.Default, factory ) {}
	}
}