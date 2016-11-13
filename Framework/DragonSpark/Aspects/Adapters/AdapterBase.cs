using DragonSpark.Extensions;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using System.Runtime.InteropServices;

namespace DragonSpark.Aspects.Adapters
{
	public abstract class AdapterBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IAdapter
	{
		protected static IParameterizedSource<object, TParameter> DefaultCoercer { get; } = CastCoercer<TParameter>.Default;
		
		readonly IParameterizedSource<object, TParameter> coercer;

		protected AdapterBase() : this( DefaultCoercer ) {}

		protected AdapterBase( IParameterizedSource<object, TParameter> coercer )
		{
			this.coercer = coercer;
		}

		object IParameterizedSource<object, object>.Get( [Optional] object parameter )
		{
			var coerced = Coerce( parameter );
			var result = (object)Get( coerced.As<TParameter>() ) ?? parameter;
			return result;
		}

		protected TParameter Coerce( [Optional] object parameter ) => coercer.Get( parameter );
	}
}