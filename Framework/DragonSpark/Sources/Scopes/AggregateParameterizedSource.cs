using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Sources.Scopes
{
	public class AggregateParameterizedSource<T> : AggregateParameterizedSource<object, T>
	{
		public AggregateParameterizedSource( Func<object, T> seed, IEnumerable<IAlteration<T>> alterations ) : base( seed, alterations ) {}
	}

	public class AggregateParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>
	{
		readonly IEnumerable<IAlteration<TResult>> alterations;

		[UsedImplicitly]
		public AggregateParameterizedSource( Func<TParameter, TResult> seed, IEnumerable<IAlteration<TResult>> alterations ) : base( seed )
		{
			this.alterations = alterations;
		}

		public override TResult Get( TParameter parameter ) => 
			alterations.Aggregate( base.Get( parameter ), ( current, transformer ) => transformer.Get( current ) );
	}
}