using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Specifications
{
	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<T, bool> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate ) : base( Where<T>.Always )
		{
			this.@delegate = @delegate;
		}

		public override bool IsSatisfiedBy( T parameter ) => @delegate( parameter );
	}
}