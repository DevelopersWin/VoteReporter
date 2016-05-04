using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class DecoratedSpecification<T> : DelegatedSpecification<T>
	{
		readonly ISpecification inner;
		public DecoratedSpecification( ISpecification inner ) : this( inner, Default<T>.Boxed ) {}

		public DecoratedSpecification( ISpecification inner, Func<T, object> projection ) : base( arg => inner.IsSatisfiedBy( arg.With( projection ) ) )
		{
			this.inner = inner;
		}
	}

	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<T, bool> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( T parameter ) => @delegate( parameter );
	}

	public class OnlyOnceSpecification : OnlyOnceSpecification<object> {}

	public class OnlyOnceSpecification<T> : SpecificationBase<T>
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		public OnlyOnceSpecification() {}

		public override bool IsSatisfiedBy( T parameter ) => monitor.Apply();
	}
}