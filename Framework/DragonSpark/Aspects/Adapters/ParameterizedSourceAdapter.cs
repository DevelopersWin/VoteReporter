using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	public class ParameterizedSourceAdapter<TParameter, TResult> : DelegatedAdapter<TParameter, TResult>, IParameterizedSourceAdapter
	{
		public ParameterizedSourceAdapter( IParameterizedSource<TParameter, TResult> source )
			: base( source, SourceCoercer<ICoercerAdapter>.Default.Get( source )?.To( Coercer ) ?? Coercer ) {}
	}
}