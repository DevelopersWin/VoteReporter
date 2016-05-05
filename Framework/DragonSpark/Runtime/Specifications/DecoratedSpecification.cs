using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using DragonSpark.Activation;

namespace DragonSpark.Runtime.Specifications
{
	public class DecoratedSpecification<T> : DelegatedSpecification<T>
	{
		public DecoratedSpecification( ISpecification inner ) : this( inner, Default<T>.Boxed ) {}

		public DecoratedSpecification( ISpecification inner, Func<T, object> projection ) : this( inner, projection, Coercer<T>.Instance ) {}
		public DecoratedSpecification( ISpecification inner, ICoercer<T> coercer ) : this( inner, Default<T>.Boxed, coercer ) {}
		public DecoratedSpecification( ISpecification inner, Func<T, object> projection, ICoercer<T> coercer ) : base( arg => inner.IsSatisfiedBy( arg.With( projection ) ), coercer ) {}
	}

	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<T, bool> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate ) : this( @delegate, Coercer<T>.Instance ) {}

		public DelegatedSpecification( Func<T, bool> @delegate, ICoercer<T> coercer ) : base( coercer )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( T parameter ) => @delegate( parameter );
	}

	public class OnlyOnceSpecification : OnlyOnceSpecification<object> {}

	public class OnlyOnceSpecification<T> : SpecificationBase<T>
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		public override bool IsSatisfiedBy( T parameter ) => monitor.Apply();
	}
}