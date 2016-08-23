using System.Runtime.InteropServices;

namespace DragonSpark
{
	public class Coercer<T> : CoercerBase<T>
	{
		public static Coercer<T> Default { get; } = new Coercer<T>();
		protected Coercer() {}

		protected override T PerformCoercion( [Optional]object parameter ) => default(T);
	}
}