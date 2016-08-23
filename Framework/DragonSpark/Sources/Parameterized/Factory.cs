using System;

namespace DragonSpark.Sources.Parameterized
{
	//public sealed class AllKnownSources

	public class ProjectedSource<TBase, TFrom, TTo> : ParameterizedSourceBase<TBase, TTo> where TFrom : TBase
	{
		readonly Func<TFrom, TTo> convert;

		public ProjectedSource( Func<TFrom, TTo> convert )
		{
			this.convert = convert;
		}

		public override TTo Get( TBase parameter ) => parameter is TFrom ? convert( (TFrom)parameter ) : default(TTo);
	}
}