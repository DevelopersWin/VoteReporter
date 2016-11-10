using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources
{
	public sealed class SourceCoercer : IParameterizedSource<object>
	{
		public static SourceCoercer Default { get; } = new SourceCoercer();
		SourceCoercer() {}

		public object Get( [Optional]object parameter )
		{
			var source = parameter as ISource;
			var result = source?.Get() ?? parameter;
			return result;
		}
	}

	public sealed class SourceCoercer<T> : CoercerBase<T>
	{
		public static SourceCoercer<T> Default { get; } = new SourceCoercer<T>();
		SourceCoercer() {}

		protected override T Coerce( object parameter )
		{
			var source = parameter as ISource<T>;
			var result = source != null ? source.Get() : default(T);
			return result;
		}
	}
}