using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System.Runtime.InteropServices;

namespace DragonSpark.Aspects.Adapters
{
	public abstract class AdapterBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IAdapter
	{
		protected TResult GetGeneral( object parameter ) => (TResult)( parameter is TParameter ? (object)Get( parameter.To<TParameter>() ) ?? parameter : parameter );

		object IParameterizedSource<object, object>.Get( [Optional] object parameter ) => GetGeneral( parameter );
	}
}