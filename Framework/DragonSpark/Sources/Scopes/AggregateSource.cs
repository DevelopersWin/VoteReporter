using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Linq;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Sources.Scopes
{
	public class AggregateSource<T> : DelegatedSource<T>
	{
		readonly static Func<T> DefaultSeed = Activator.Default.Get<T>;

		readonly IEnumerable<IAlteration<T>> alterations;

		public AggregateSource( IEnumerable<IAlteration<T>> alterations ) : this( DefaultSeed, alterations ) {}
		public AggregateSource( Func<T> seed, IEnumerable<IAlteration<T>> alterations ) : base( seed )
		{
			this.alterations = alterations;
		}

		public override T Get() => 
			alterations.Aggregate( base.Get(), ( current, transformer ) => transformer.Get( current ) );
	}
}