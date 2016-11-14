using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Specifications
{
	public class SpecificationCache<TKey, TSpecification> : Cache<TKey, ISpecification<TSpecification>> where TKey : class
	{
		public SpecificationCache( Func<TKey, ISpecification<TSpecification>> create ) : base( create ) {}

		public sealed class DelegateCoercer : ParameterizedSourceBase<ISpecification<TSpecification>, Func<TSpecification, bool>>
		{
			public static DelegateCoercer Default { get; } = new DelegateCoercer();
			DelegateCoercer() {}

			public override Func<TSpecification, bool> Get( ISpecification<TSpecification> parameter ) => parameter.ToDelegate();
		}
	}
}