using System.Runtime.InteropServices;
using DragonSpark.Extensions;

namespace DragonSpark
{
	public abstract class CoercerBase<T> : ICoercer<T>
	{
		public T Coerce( [Optional]object parameter ) => parameter is T ? (T)parameter : parameter.IsAssignedOrValue() ? PerformCoercion( parameter ) : default(T);

		protected abstract T PerformCoercion( object parameter );
	}
}