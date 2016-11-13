using DragonSpark.Extensions;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using System.Runtime.InteropServices;

namespace DragonSpark.Aspects.Adapters
{
	public abstract class AdapterBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IAdapter
	{
		protected static CastCoercer<object, TParameter> Coercer { get; } = CastCoercer<object, TParameter>.Default;

		readonly IParameterizedSource<object, TParameter> coercer;

		protected AdapterBase() : this( Coercer ) {}

		protected AdapterBase( IParameterizedSource<object, TParameter> coercer )
		{
			this.coercer = coercer;
		}

		object IParameterizedSource<object, object>.Get( [Optional] object parameter ) => GetGeneral<object>( parameter );

		protected T GetGeneral<T>( object parameter )
		{
			var coerced = coercer.Get( parameter );
			var general = coerced.IsAssigned() ? (object)Get( coerced ) ?? parameter : parameter;
			var result = general.As<T>();
			return result;
		}
	}
}