using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Specifications
{
	public sealed class AssignedSpecification<T> : SpecificationBase<T>
	{
		public static ISpecification<T> Default { get; } = new AssignedSpecification<T>();
		AssignedSpecification() : base( Where<T>.Always ) {}
	
		public override bool IsSatisfiedBy( [Optional]T parameter ) => parameter.IsAssigned();
	}

	public sealed class DelegatedAssignedSpecification<T> : SpecificationBase<Func<T>>
	{
		public static DelegatedAssignedSpecification<T> Default { get; } = new DelegatedAssignedSpecification<T>();
		DelegatedAssignedSpecification() : this( AssignedSpecification<T>.Default.IsSatisfiedBy ) {}

		readonly Func<T, bool> inner;

		public DelegatedAssignedSpecification( Func<T, bool> inner )
		{
			this.inner = inner;
		}

		public override bool IsSatisfiedBy( Func<T> parameter ) => inner( parameter() );
	}
}