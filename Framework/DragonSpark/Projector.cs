using DragonSpark.Extensions;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark
{
	public class Projector<TFrom, TTo> : CoercerBase<TTo>
	{
		readonly Func<TFrom, TTo> projection;
		public Projector( Func<TFrom, TTo> projection )
		{
			this.projection = projection;
		}

		protected override TTo PerformCoercion( [Optional]object parameter ) => parameter.AsTo( projection );
	}
}