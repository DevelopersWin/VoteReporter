using DragonSpark.Extensions;
using System;

namespace DragonSpark
{
	public class Projector<TFrom, TTo> : CoercerBase<TTo>
	{
		readonly Func<TFrom, TTo> projection;
		public Projector( Func<TFrom, TTo> projection )
		{
			this.projection = projection;
		}

		protected override TTo Apply( object parameter ) => parameter.AsTo( projection );
	}
}