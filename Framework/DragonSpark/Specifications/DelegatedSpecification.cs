using DragonSpark.Sources;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Specifications
{
	public sealed class DeferredSpecification<T> : SpecificationBase<T>
	{
		readonly Func<ISpecification<T>> source;
		public DeferredSpecification( Func<ISpecification<T>> source )
		{
			this.source = source;
		}

		public override bool IsSatisfiedBy( T parameter ) => source().IsSatisfiedBy( parameter );
	}

	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<Func<T, bool>> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate ) : this( @delegate.Self ) {}

		public DelegatedSpecification( Func<Func<T, bool>> @delegate )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( [Optional]T parameter ) => @delegate().Invoke( parameter );
	}
}