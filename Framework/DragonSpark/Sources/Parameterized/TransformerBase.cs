namespace DragonSpark.Sources.Parameterized
{
	public abstract class TransformerBase<T> : ParameterizedSourceBase<T, T>, ITransformer<T> {}

	public class DelegatedTransformer<T> : TransformerBase<T>
	{
		readonly Transform<T> source;

		public DelegatedTransformer( Transform<T> source )
		{
			this.source = source;
		}

		public override T Get( T parameter ) => source( parameter );
	}
}