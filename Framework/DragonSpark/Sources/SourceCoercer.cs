using System.Runtime.InteropServices;
using DragonSpark.Extensions;

namespace DragonSpark.Sources
{
	public class SourceCoercer<T> : ICoercer<T>
	{
		public static SourceCoercer<T> Default { get; } = new SourceCoercer<T>();
		SourceCoercer() {}

		public T Coerce( [Optional]object parameter )
		{
			var store = parameter as ISource<T>;
			var result = store != null ? store.Get() : parameter.As<T>();
			return result;
		}
	}
}