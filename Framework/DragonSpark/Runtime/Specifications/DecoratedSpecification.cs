using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class DecoratedSpecification<T> : DecoratedSpecification, ISpecification<T>
	{
		public DecoratedSpecification( ISpecification<T> inner ) : base( inner ) {}

		public bool IsSatisfiedBy( T parameter ) => base.IsSatisfiedBy( parameter );
	}

	public interface ISpecificationAware
	{
		ISpecification Specification { get; }
	}

	public class DecoratedSpecification : DelegatedSpecification, ISpecificationAware
	{
		public DecoratedSpecification( ISpecification inner ) : base( inner.IsSatisfiedBy )
		{
			Specification = inner;
		}

		public ISpecification Specification { get; }
	}

	public class ProjectedSpecification<T> : ProjectedSpecification, ISpecification<T>
	{
		public ProjectedSpecification( ISpecification specification ) : base( specification ) {}

		public ProjectedSpecification( ISpecification specification, Func<T, object> projection ) : this( specification, projection, Coercer<T>.Instance ) {}
		public ProjectedSpecification( ISpecification specification, ICoercer<T> coercer ) : this( specification, Default<T>.Boxed, coercer ) {}
		public ProjectedSpecification( ISpecification specification, Func<T, object> projection, ICoercer<T> coercer ) : base( specification, o => projection( coercer.Coerce( o ) ) ) {}

		public bool IsSatisfiedBy( T parameter ) => base.IsSatisfiedBy( parameter );
	}

	public class ProjectedSpecification : DecoratedSpecification
	{
		readonly Func<object, object> projection;

		public ProjectedSpecification( ISpecification specification ) : this( specification, Default<object>.Boxed ) {}

		public ProjectedSpecification( ISpecification specification, Func<object, object> projection ) : base( specification )
		{
			this.projection = projection;
		}

		public override bool IsSatisfiedBy( object parameter ) => base.IsSatisfiedBy( parameter.With( projection ) );
	}

	public abstract class SpecificationBase : SpecificationBase<object>
	{
		protected SpecificationBase() {}
		protected SpecificationBase( ICoercer<object> coercer ) : base( coercer ) {}
		protected SpecificationBase( Func<object, object> coercer ) : base( coercer ) {}
	}

	public abstract class SpecificationBase<T> : ISpecification<T>
	{
		readonly Func<object, T> coercer;

		protected SpecificationBase() : this( Coercer<T>.Instance ) {}

		protected SpecificationBase(  ICoercer<T> coercer ) : this( coercer.Coerce ) {}

		protected SpecificationBase( Func<object, T> coercer )
		{
			this.coercer = coercer;
		}

		public abstract bool IsSatisfiedBy( T parameter );

		bool ISpecification.IsSatisfiedBy( object parameter )
		{
			var coerced = coercer( parameter );
			var result = IsSatisfiedBy( coerced );
			return result;
		}
	}

	public class DelegatedSpecification<T> : DelegatedSpecification, ISpecification<T>
	{
		public DelegatedSpecification( Func<T, bool> @delegate ) : base( o => @delegate( (T)o ) ) {}

		public bool IsSatisfiedBy( T parameter ) => base.IsSatisfiedBy( parameter );
	}

	public class DelegatedSpecification : SpecificationBase
	{
		readonly Func<object, bool> @delegate;

		public DelegatedSpecification( Func<object, bool> @delegate )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( object parameter ) => @delegate( parameter );
	}

	/*public abstract class SpecificationBase<T> : SpecificationBase, ISpecification<T>
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
	}*/

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