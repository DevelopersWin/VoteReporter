using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using System;

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

	public class OncePerParameterSpecification<T> : SpecificationBase<T> where T : class
	{
		public static OncePerParameterSpecification<T> Instance { get; } = new OncePerParameterSpecification<T>();

		public override bool IsSatisfiedBy( T parameter ) => parameter.Get( Condition.Property ).Apply();

		class Condition : Values.Condition
		{
			public new static Condition Property { get; } = new Condition();
		}
	}

	public class OnlyOnceSpecification : OnlyOnceSpecification<object> {}

	public class OnlyOnceSpecification<T> : SpecificationBase<T>
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		public override bool IsSatisfiedBy( T parameter ) => monitor.Apply();
	}
}