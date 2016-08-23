namespace DragonSpark.Sources.Parameterized
{
	public class SelfTransformer<T> : TransformerBase<T>
	{
		public static SelfTransformer<T> Default { get; } = new SelfTransformer<T>();

		public override T Get( T parameter ) => parameter;
	}
}