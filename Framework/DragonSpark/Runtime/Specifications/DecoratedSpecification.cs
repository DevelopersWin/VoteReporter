using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using DragonSpark.Activation.Sources;
using DragonSpark.Activation.Sources.Caching;

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

	public class OncePerParameterSpecification<T> : ConditionMonitorSpecificationBase<T> where T : class
	{
		public OncePerParameterSpecification() : this( new Condition<T>() ) {}

		public OncePerParameterSpecification( ICache<T, ConditionMonitor> cache ) : base( cache.ToDelegate() ) {}
	}

	public class OncePerScopeSpecification<T> : ConditionMonitorSpecificationBase<T>
	{
		public OncePerScopeSpecification() : this( new Scope<ConditionMonitor>( Factory.Scope( () => new ConditionMonitor() ) ) ) {}

		public OncePerScopeSpecification( ISource<ConditionMonitor> source ) : base( source.Wrap<T, ConditionMonitor>() ) {}
	}

	public abstract class ConditionMonitorSpecificationBase<T> : SpecificationBase<T>
	{
		readonly Func<T, ConditionMonitor> source;
		protected ConditionMonitorSpecificationBase( Func<T, ConditionMonitor> source )
		{
			this.source = source;
		}

		public override bool IsSatisfiedBy( T parameter )
		{
			var conditionMonitor = source( parameter );
			return conditionMonitor.Apply();
		}
	}

	public class OnlyOnceSpecification : OnlyOnceSpecification<object> {}

	public class OnlyOnceSpecification<T> : ConditionMonitorSpecificationBase<T>
	{
		public OnlyOnceSpecification() : this( new ConditionMonitor() ) {}

		public OnlyOnceSpecification( ConditionMonitor monitor ) : base( monitor.Wrap<T, ConditionMonitor>() ) {}

		
	}
}