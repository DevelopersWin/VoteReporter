using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class DecoratedSpecification<T> : DelegatedSpecification<T>
	{
		public DecoratedSpecification( ISpecification<T> inner ) : this( inner, Coercer<T>.Instance ) {}
		public DecoratedSpecification( ISpecification<T> inner, ICoercer<T> coercer ) : base( inner.IsSatisfiedBy, coercer ) {}
	}

	public class ProjectedSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification specification;
		readonly Func<T, object> projection;

		public ProjectedSpecification( ISpecification specification ) : this( specification, Default<T>.Boxed ) {}

		public ProjectedSpecification( ISpecification specification, Func<T, object> projection )
		{
			this.specification = specification;
			this.projection = projection;
		}

		public override bool IsSatisfiedBy( T parameter ) => specification.IsSatisfiedBy( projection( parameter ) );
	}

	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<T, bool> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate ) : this( @delegate, Coercer<T>.Instance ) { }

		public DelegatedSpecification( Func<T, bool> @delegate, ICoercer<T> coercer ) : base( coercer )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( T parameter ) => @delegate( parameter );
	}

	public abstract class SpecificationBase<T> : SpecificationBase, ISpecification<T>
	{
		readonly Func<object, T> coercer;

		protected SpecificationBase() : this( Coercer<T>.Instance ) {}
		protected SpecificationBase( ICoercer<T> coercer ) : this( coercer.Coerce ) {}

		protected SpecificationBase( Func<object, T> coercer )
		{
			this.coercer = coercer;
		}

		public abstract bool IsSatisfiedBy( T parameter );

		public override bool IsSatisfiedBy( object parameter ) => IsSatisfiedBy( coercer( parameter ) );
	}

	/*public class DecoratedSpecification : SpecificationBase
	{
		readonly ISpecification specification;

		public DecoratedSpecification( ISpecification specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( parameter );
	}

	public class DelegatedSpecification<T> : SpecificationBase, ISpecification<T>
	{
		readonly Func<T, bool> @delegate;
		readonly Func<object, T> coercer;
		readonly Func<T, object> projection;
		// public DelegatedSpecification( ISpecification inner ) : this( arg => inner.IsSatisfiedBy( arg ) ) {}

		public DelegatedSpecification( Func<T, bool> @delegate ) : this( @delegate, Coercer<T>.Instance.Coerce, Default<T>.Boxed ) {}

		public DelegatedSpecification( Func<T, bool> @delegate, Func<T, object> projection  ) : this( @delegate, Coercer<T>.Instance.Coerce, projection ) {}

		public DelegatedSpecification( Func<T, bool> @delegate, Func<object, T> coercer, Func<T, object> projection  )
		{
			this.@delegate = @delegate;
			this.coercer = coercer;
			this.projection = projection;
		}

		public override bool IsSatisfiedBy( object parameter )
		{
			var coerced = coercer( parameter );
			var projected = projection( coerced );
			var result = new DelegatedSpecification( o => @delegate( (T)o ) ).IsSatisfiedBy( projected );
			return result;
		}

		public bool IsSatisfiedBy( T parameter ) => @delegate( parameter );
	}

	public class DelegatedSpecification : SpecificationBase
	{
		readonly Func<object, bool> @delegate;

		public DelegatedSpecification( ISpecification inner ) : this( inner.IsSatisfiedBy ) {}

		public DelegatedSpecification( Func<object, bool> @delegate )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( object parameter ) => @delegate( parameter );
	}*/

	public interface IApplyAware
	{
		void Apply();
	}

	public class OnlyOnceSpecification : SpecificationBase, IApplyAware
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		public override bool IsSatisfiedBy( object parameter ) => !monitor.IsApplied;

		public void Apply() => monitor.Apply();
	}
}