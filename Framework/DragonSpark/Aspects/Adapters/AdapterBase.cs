using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System.Runtime.InteropServices;

namespace DragonSpark.Aspects.Adapters
{
	public abstract class AdapterBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IAdapter
	{
		object IParameterizedSource<object, object>.Get( [Optional] object parameter ) =>
			parameter is TParameter ? (object)Get( parameter.To<TParameter>() ) ?? parameter : parameter;
	}
}