namespace DragonSpark.Aspects.Adapters
{
	public abstract class CommandAdapterBase<T> : AdapterBase<T, object>
	{
		protected abstract void Execute( T parameter );

		public sealed override object Get( T parameter )
		{
			Execute( parameter );
			return null;
		}
	}
}