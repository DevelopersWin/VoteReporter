using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class IntroduceGeneralizedParameterizedSource : IntroduceInterfaceAspectBase
	{
		readonly static Func<Type, Func<object, object>> Factory = new ImplementationCache( ParameterizedSourceTypeDefinition.Default.ReferencedType ).ToCache().ToDelegate();

		public IntroduceGeneralizedParameterizedSource() : this( typeof(ParameterizedSourceAdapter<,>) ) {}

		public IntroduceGeneralizedParameterizedSource( Type implementationType ) : this( Factory( implementationType ) ) {}

		[UsedImplicitly]
		public IntroduceGeneralizedParameterizedSource( Func<object, object> factory ) : base( GeneralizedParameterizedSourceTypeDefinition.Default, factory ) {}
	}
}