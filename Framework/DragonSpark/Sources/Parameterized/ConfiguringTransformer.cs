using System;

namespace DragonSpark.Sources.Parameterized
{
	public class ConfiguringTransformer<T> : TransformerBase<T>
	{
		readonly Action<T> configure;

		public ConfiguringTransformer( Action<T> configure )
		{
			this.configure = configure;
		}

		public override T Get( T parameter )
		{
			configure( parameter );
			return parameter;
		}
	}
}