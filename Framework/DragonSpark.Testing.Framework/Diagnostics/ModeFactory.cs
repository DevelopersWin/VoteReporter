using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	sealed class ModeFactory<T> : ParameterizedSourceBase<ImmutableArray<T>, T>
	{
		public static ModeFactory<T> DefaultNested { get; } = new ModeFactory<T>();
		public override T Get( ImmutableArray<T> parameter ) => parameter.ToArray().GroupBy( n => n ).OrderByDescending( g => g.Count() ).Select( g => g.Key ).FirstOrDefault();
	}
}