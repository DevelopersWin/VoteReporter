using DragonSpark.Sources;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Specifications
{
	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<Func<T, bool>> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate ) : this( @delegate.Self ) {}

		public DelegatedSpecification( Func<Func<T, bool>> @delegate ) : base( Where<T>.Always )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( T parameter ) => @delegate().Invoke( parameter );
	}
}