using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class IntroduceGeneralizedParameterizedSource : IntroduceInterfaceAspectBase
	{
		readonly static Func<Type, Func<object, object>> Factory = new ImplementationCache( ParameterizedSourceTypeDefinition.Default.ReferencedType ).ToCache().ToDelegate();

		public IntroduceGeneralizedParameterizedSource() : this( typeof(DefaultGeneralizedParameterizedSource<,>) ) {}

		public IntroduceGeneralizedParameterizedSource( Type implementationType ) : this( Factory( implementationType ) ) {}

		[UsedImplicitly]
		public IntroduceGeneralizedParameterizedSource( Func<object, object> factory ) : base( GeneralizedParameterizedSourceTypeDefinition.Default.Inverse(), factory, GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType ) {}
	}

	sealed class DefaultGeneralizedParameterizedSource<TParameter, TResult> : DelegatedAdapter<TParameter, TResult>
	{
		public DefaultGeneralizedParameterizedSource( IParameterizedSource<TParameter, TResult> implementation ) 
			: base( 
				  SourceCoercer<ICoercerAdapter>.Default.Get( implementation )?.To( DefaultCoercer ) ?? DefaultCoercer, 
				  new ParameterizedSourceAdapter<TParameter, TResult>( implementation ).Get ) {}
	}
}