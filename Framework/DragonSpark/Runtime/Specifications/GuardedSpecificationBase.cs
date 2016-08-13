using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class InverseSpecification : InverseSpecification<object>
	{
		public InverseSpecification( ISpecification<object> inner ) : base( inner ) {}
	}

	public class InverseSpecification<T> : DecoratedSpecification<T>
	{
		public InverseSpecification( ISpecification<T> inner ) : base( inner ) {}

		public override bool IsSatisfiedBy( T parameter ) => !base.IsSatisfiedBy( parameter );
	}

	public class AssignedSpecification<T> : SpecificationBase<T>
	{
		public static ISpecification<T> Instance { get; } = new AssignedSpecification<T>();
		AssignedSpecification() : base( Where<T>.Always ) {}
	
		public override bool IsSatisfiedBy( T parameter ) => parameter.IsAssigned();
	}

	public abstract class SpecificationBase<T> : ISpecification<T>
	{
		readonly Coerce<T> coercer;
		readonly Func<T, bool> apply;

		protected SpecificationBase() : this( Defaults<T>.Coercer ) {}

		protected SpecificationBase( Coerce<T> coercer ) : this( coercer, Where<T>.Assigned ) {}

		protected SpecificationBase( Func<T, bool> apply ) : this( Defaults<T>.Coercer, apply ) {}

		protected SpecificationBase( Coerce<T> coercer, Func<T, bool> apply )
		{
			this.coercer = coercer;
			this.apply = apply;
		}

		public abstract bool IsSatisfiedBy( T parameter );

		bool ISpecification.IsSatisfiedBy( object parameter ) => Coerce( parameter );

		protected virtual bool Coerce( object parameter )
		{
			var coerced = coercer( parameter );
			var result = apply( coerced ) && IsSatisfiedBy( coerced );
			return result;
		}

		// protected virtual bool IsSatisfiedByCoerced( T parameter ) => IsSatisfiedBy( parameter );
	}

	/*public abstract class GuardedSpecificationBase<T> : SpecificationBase<T>
	{
		protected GuardedSpecificationBase() : this( Defaults<T>.Coercer ) {}
		protected GuardedSpecificationBase( Coerce<T> coercer ) : base( coercer, Where<T>.Assigned ) {}
	}*/
}