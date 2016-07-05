using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using System;
using System.Linq;

namespace DragonSpark.Runtime.Specifications
{
	public class AssignableSpecification : GuardedSpecificationBase<Type>
	{
		readonly object[] arguments;
		public AssignableSpecification( params object[] arguments )
		{
			this.arguments = arguments;
		}

		public override bool IsSatisfiedBy( Type parameter ) => arguments.Any( parameter.Adapt().IsInstanceOfType );
	}

	public class CanCreateSpecification<T> : CanCreateSpecification<T, object>
	{
		public CanCreateSpecification( Func<T, object> creator ) : base( creator ) {}
		public CanCreateSpecification( Func<T, object> creator, Coerce<T> coercer ) : base( creator, coercer ) {}
	}

	public class CanCreateSpecification<TParameter, TResult> : GuardedSpecificationBase<TParameter>
	{
		readonly Func<TParameter, TResult> creator;

		public CanCreateSpecification( Func<TParameter, TResult> creator ) : this( creator, Defaults<TParameter>.Coercer ) {}
		public CanCreateSpecification( Func<TParameter, TResult> creator, Coerce<TParameter> coercer ) : base( coercer )
		{
			this.creator = creator;
		}

		[Freeze]
		public override bool IsSatisfiedBy( TParameter parameter ) => creator( parameter ).IsAssignedOrContains();
	}
}
