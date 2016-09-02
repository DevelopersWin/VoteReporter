namespace DragonSpark.Specifications
{
	/*public static class Defaults
	{
		public static Func<object, ISpecification> Query { get; } = QueryableLocator<ISpecification>.Default.Get;
	}

	public sealed class SpecificationLocator<T> : ParameterizedSourceBase<ISpecification<T>>
	{
		readonly static ISpecification<T> DefaultResult = typeof(T) == typeof(object) ? Specifications<T>.Never : Specifications<T>.Assigned;

		public static SpecificationLocator<T> Default { get; } = new SpecificationLocator<T>();
		SpecificationLocator() : this( Defaults.Query ) {}

		readonly Func<object, ISpecification> query;

		SpecificationLocator( Func<object, ISpecification> query )
		{
			this.query = query;
		}

		public override ISpecification<T> Get( object parameter )
		{
			var specification = ( parameter as ISpecification )?.Cast<T>();
			var result = specification ?? query( parameter )?.Cast<T>() ?? DefaultResult;
			return result;
		}
	}*/
}