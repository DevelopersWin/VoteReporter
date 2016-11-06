using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System.Runtime.InteropServices;

namespace DragonSpark.Runtime
{
	/*public sealed class EnumerableResultAlteration<T> : AlterationBase<IEnumerable<T>>
	{
		public static EnumerableResultAlteration<T> Default { get; } = new EnumerableResultAlteration<T>();
		EnumerableResultAlteration() {}

		public override IEnumerable<T> Get( [Optional]IEnumerable<T> parameter ) => parameter ?? Items<T>.Default;
	}*/

	public sealed class DefaultValueAlteration<T> : AlterationBase<T> where T : class
	{
		public static DefaultValueAlteration<T> Default { get; } = new DefaultValueAlteration<T>();
		DefaultValueAlteration() : this( DefaultValues.Default.GetWith<T>() ) {}

		readonly T defaultValue;

		[UsedImplicitly]
		public DefaultValueAlteration( T defaultValue )
		{
			this.defaultValue = defaultValue;
		}

		public override T Get( [Optional]T parameter ) => parameter ?? defaultValue;
	}
}
