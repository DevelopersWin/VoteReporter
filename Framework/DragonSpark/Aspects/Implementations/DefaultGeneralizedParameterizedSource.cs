using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Implementations
{
	sealed class DefaultGeneralizedParameterizedSource<TParameter, TResult> : DelegatedAdapter<TParameter, TResult>
	{
		public DefaultGeneralizedParameterizedSource( IParameterizedSource<TParameter, TResult> implementation ) 
			: base( 
				SourceCoercer<ICoercerAdapter>.Default.Get( implementation )?.To( DefaultCoercer ) ?? DefaultCoercer, 
				new ParameterizedSourceAdapter<TParameter, TResult>( implementation ).Get ) {}
	}
}