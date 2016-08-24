using System;
using System.Windows.Markup;
using DragonSpark.Extensions;
using DragonSpark.Specifications;

namespace DragonSpark.Windows.Markup
{
	public class Specification<TTarget, TProperty> : SpecificationBase<IServiceProvider>
	{
		public static Specification<TTarget, TProperty> Default { get; } = new Specification<TTarget, TProperty>();

		public override bool IsSatisfiedBy( IServiceProvider parameter ) => 
			parameter.Get<IProvideValueTarget>().With( target => target.TargetObject is TTarget && target.TargetProperty is TProperty )
			;
	}
}