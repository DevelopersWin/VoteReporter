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

	public abstract class SpecificationBase<T> : ISpecification<T>
	{
		readonly Func<object, T> coercer;

		protected SpecificationBase() : this( Coercer<T>.Instance ) {}

		protected SpecificationBase( ICoercer<T> coercer ) : this( coercer.Coerce ) {}

		protected SpecificationBase( Func<object, T> coercer )
		{
			this.coercer = coercer;
		}

		public abstract bool IsSatisfiedBy( T parameter );

		bool ISpecification.IsSatisfiedBy( object parameter ) => Coerce( parameter );

		protected bool Coerce( object parameter )
		{
			var coerced = coercer( parameter );
			var result = IsSatisfiedBy( coerced );
			return result;
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