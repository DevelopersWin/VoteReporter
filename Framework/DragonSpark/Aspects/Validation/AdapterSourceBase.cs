using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	abstract class AdapterSourceBase<T> : ProjectedSource<T, IParameterValidationAdapter> where T : class
	{
		protected AdapterSourceBase( Func<T, IParameterValidationAdapter> create ) : base( create ) {}
	}
}