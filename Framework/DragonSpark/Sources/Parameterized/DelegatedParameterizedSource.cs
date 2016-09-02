using System;

namespace DragonSpark.Sources.Parameterized
{
	/*public abstract class QueryableParameterizedSourceBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IQueryableContainer
	{
		readonly ImmutableArray<IQueryableObject> queries;

		protected QueryableParameterizedSourceBase( params object[] instances ) : this( instances.Select( o => o as IQueryableObject ?? new QueryableObject( o ) ).Fixed() ) {}
		protected QueryableParameterizedSourceBase( params IQueryableObject[] queries )
		{
			this.queries = queries.ToImmutableArray();
		}

		public virtual IEnumerator<IQueryableObject> GetEnumerator() => queries.Select( o => o.Expand() ).Concat().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}*/

	public class DelegatedParameterizedSource<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> source;

		public DelegatedParameterizedSource( Func<TParameter, TResult> source )/* : base( source.Target != null ? new QueryableObject( source.Target ).ToItem() : Items<IQueryableObject>.Default )*/
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source( parameter );
	}
}