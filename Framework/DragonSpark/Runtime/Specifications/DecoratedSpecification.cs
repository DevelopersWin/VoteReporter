using DragonSpark.Activation;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class CastingSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification specification;

		public CastingSpecification( ISpecification specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( T parameter ) => specification.IsSatisfiedBy( parameter );
	}

	public class CastingSpecification<TFrom, TTo> : SpecificationBase<TTo> where TFrom : TTo
	{
		readonly ISpecification<TFrom> specification;

		public CastingSpecification( ISpecification<TFrom> specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( TTo parameter ) => specification.IsSatisfiedBy( parameter );
	}

	public class ProjectedSpecification<TOrigin, TDestination> : SpecificationBase<TDestination>
	{
		readonly Func<TOrigin, bool> @delegate;
		readonly Coerce<TOrigin> coerce;

		public ProjectedSpecification( ISpecification<TOrigin> inner, Func<TDestination, TOrigin> projection ) : this( inner.ToSpecificationDelegate(), projection ) {}

		public ProjectedSpecification( Func<TOrigin, bool> @delegate, Func<TDestination, TOrigin> projection ) : this( @delegate, new Projector<TDestination, TOrigin>( projection ).ToDelegate() ) {}

		ProjectedSpecification( Func<TOrigin, bool> @delegate, Coerce<TOrigin> coerce )
		{
			this.@delegate = @delegate;
			this.coerce = coerce;
		}

		public override bool IsSatisfiedBy( TDestination parameter ) => @delegate( coerce( parameter ) );
	}

	public class DecoratedSpecification<T> : DelegatedSpecification<T>
	{
		readonly ISpecification<T> specification;

		public DecoratedSpecification( ISpecification<T> specification ) : base( specification.ToSpecificationDelegate() )
		{
			this.specification = specification;
		}

		protected override bool Coerce( object parameter ) => parameter is T ? IsSatisfiedBy( (T)parameter ) : specification.IsSatisfiedBy( parameter );
	}

	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<T, bool> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate ) : base( Where<T>.Always )
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

		public override bool IsSatisfiedBy( T parameter ) => source( parameter ).Apply();
	}

	public class OnlyOnceSpecification : OnlyOnceSpecification<object> {}

	public class OnlyOnceSpecification<T> : ConditionMonitorSpecificationBase<T>
	{
		public OnlyOnceSpecification() : this( new ConditionMonitor() ) {}

		public OnlyOnceSpecification( ConditionMonitor monitor ) : base( monitor.Wrap<T, ConditionMonitor>() ) {}

		
	}
}