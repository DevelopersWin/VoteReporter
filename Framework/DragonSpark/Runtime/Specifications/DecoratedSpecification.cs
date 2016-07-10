using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class DecoratedSpecification<T> : DelegatedSpecification<T>
	{
		public DecoratedSpecification( ISpecification inner ) : this( inner, Delegates<T>.Object ) {}

		public DecoratedSpecification( ISpecification inner, Func<T, object> projection ) : this( inner, projection, Defaults<T>.Coercer ) {}
		public DecoratedSpecification( ISpecification inner, Coerce<T> coercer ) : this( inner, Delegates<T>.Object, coercer ) {}
		public DecoratedSpecification( ISpecification inner, Func<T, object> projection, Coerce<T> coercer ) : base( arg => inner.IsSatisfiedBy( arg.With( projection ) ), coercer ) {}
	}

	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<T, bool> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate ) : this( @delegate, Defaults<T>.Coercer ) {}

		public DelegatedSpecification( Func<T, bool> @delegate, Coerce<T> coercer ) : base( coercer )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( T parameter ) => @delegate( parameter );
	}

	public class OncePerParameterSpecification<T> : SpecificationBase<T> where T : class
	{
		readonly ICache<T, ConditionMonitor> cache;
		
		public OncePerParameterSpecification() : this( new Condition<T>() ) {}

		public OncePerParameterSpecification( ICache<T, ConditionMonitor> cache )
		{
			this.cache = cache;
		}

		public override bool IsSatisfiedBy( T parameter ) => cache.Get( parameter ).Apply();
	}

	public class OnlyOnceSpecification : ConditionMonitorSpecification<object> {}

	public class ConditionMonitorSpecification<T> : SpecificationBase<T>
	{
		readonly ConditionMonitor monitor;

		public ConditionMonitorSpecification() : this( new ConditionMonitor() ) {}

		public ConditionMonitorSpecification( ConditionMonitor monitor )
		{
			this.monitor = monitor;
		}

		public override bool IsSatisfiedBy( T parameter ) => monitor.Apply();
	}
}