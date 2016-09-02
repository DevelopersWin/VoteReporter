namespace DragonSpark.Sources
{
	/*public interface IQueryableContainer : IEnumerable<IQueryableObject> {}

	public interface IQueryableObject
	{
		object Instance { get; }
	}

	public sealed class QueryableObject : IQueryableObject
	{
		public QueryableObject( object instance )
		{
			Instance = instance;
		}

		public object Instance { get; }
	}

	public sealed class QueryableLocator<T> : ParameterizedSourceBase<T>
	{
		public static QueryableLocator<T> Default { get; } = new QueryableLocator<T>();
		QueryableLocator() {}

		public override T Get( object parameter )
		{
			var container = parameter as IQueryableContainer;
			var result = container != null ? container.Select( o => o.Instance ).FirstOrDefaultOfType<T>() : default(T);
			return result;
		}
	}*/
}