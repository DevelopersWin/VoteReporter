using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Diagnostics.Exceptions
{
	public abstract class RetryTimeBase : ParameterizedSourceBase<int, TimeSpan>
	{
		readonly Transform<int> time;

		protected RetryTimeBase( Transform<int> time )
		{
			this.time = time;
		}

		public override TimeSpan Get( int parameter ) => TimeSpan.FromSeconds( time( parameter ) );
	}
}