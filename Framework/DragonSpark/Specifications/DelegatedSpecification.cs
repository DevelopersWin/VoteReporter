using DragonSpark.Sources;
using DragonSpark.TypeSystem;
using System;
using System.Runtime.InteropServices;

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

	public class FixedDelegatedSpecification<T> : SpecificationBase<object>
	{
		readonly Func<T, bool> source;
		readonly Func<T> parameterSource;

		public FixedDelegatedSpecification( ISpecification<T> specification, T parameter ) : this( specification, Factory.For( parameter ) ) {}
		public FixedDelegatedSpecification( ISpecification<T> specification, Func<T> parameterSource ) : this( specification.ToSpecificationDelegate(), parameterSource ) {}

		public FixedDelegatedSpecification( Func<T, bool> source, T parameter ) : this( source, Factory.For( parameter ) ) {}
		public FixedDelegatedSpecification( Func<T, bool> source, Func<T> parameterSource ) : base( Where<object>.Always )
		{
			this.source = source;
			this.parameterSource = parameterSource;
		}

		public override bool IsSatisfiedBy( [Optional]object _ ) => source( parameterSource() );
	}
}