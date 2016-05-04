using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class DecoratedSpecification<T> : DelegatedSpecification<T>, ISpecificationAware
	{
		public DecoratedSpecification( ISpecification inner ) : this( inner, Default<T>.Boxed ) {}

		public DecoratedSpecification( ISpecification inner, Func<T, object> projection ) : base( arg => inner.IsSatisfiedBy( arg.With( projection ) ) )
		{
			Specification = inner;
		}

		public ISpecification Specification { get; }
	}

	public interface ISpecificationAware
	{
		ISpecification Specification { get; }
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

	public interface IApplyAware
	{
		void Apply();
	}

	public class OnlyOnceSpecification : OnlyOnceSpecification<object> {}

	public class OnlyOnceSpecification<T> : SpecificationBase<T>, IApplyAware
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		public OnlyOnceSpecification() {}

		public override bool IsSatisfiedBy( T parameter ) => !monitor.IsApplied;

		public void Apply() => monitor.Apply();
	}
}