using System;

namespace DragonSpark.Sources.Parameterized
{
	public class ProjectedSource<TFrom, TTo> : ProjectedSource<object, TFrom, TTo>
	{
		public ProjectedSource( Func<TFrom, TTo> convert ) : base( convert ) {}
	}
}