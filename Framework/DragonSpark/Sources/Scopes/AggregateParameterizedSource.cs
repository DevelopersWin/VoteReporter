using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Sources.Scopes
{
	/*public class AggregateParameterizedSource<T> : AggregateParameterizedSource<object, T>
	{
		public AggregateParameterizedSource( Func<object, T> seed, IEnumerable<IAlteration<T>> configurations ) : base( seed, configurations ) {}
	}*/

	public class AggregateParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>
	{
		readonly IEnumerable<IAlteration<TResult>> configurations;

		[UsedImplicitly]
		public AggregateParameterizedSource( Func<TParameter, TResult> seed, IEnumerable<IAlteration<TResult>> configurations ) : base( seed )
		{
			this.configurations = configurations;
		}

		public override TResult Get( TParameter parameter ) => 
			configurations.Aggregate( base.Get( parameter ), ( current, transformer ) => transformer.Get( current ) );
	}
}