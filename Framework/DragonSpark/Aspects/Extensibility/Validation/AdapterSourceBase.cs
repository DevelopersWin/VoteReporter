using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	abstract class AdapterSourceBase<T> : ProjectedSource<T, IParameterValidationAdapter>
	{
		protected AdapterSourceBase( Func<T, IParameterValidationAdapter> create ) : base( create ) {}
	}
}