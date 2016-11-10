using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System.Runtime.InteropServices;

namespace DragonSpark.Aspects.Adapters
{
	public abstract class AdapterBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IAdapter<TParameter, TResult>
	{
		object IAdapter.Get( [Optional] object parameter ) =>
			parameter is TParameter ? (object)Get( parameter.AsValid<TParameter>() ) ?? parameter : parameter;
	}
}