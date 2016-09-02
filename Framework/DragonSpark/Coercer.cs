using System.Runtime.InteropServices;

namespace DragonSpark
{
	public class Coercer<T> : CoercerBase<T>
	{
		public static Coercer<T> Default { get; } = new Coercer<T>();
		protected Coercer() {}

		protected override T Apply( [Optional]object parameter ) => default(T);
	}
}