using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Runtime.Specifications
{
	public class InverseSpecification : ISpecification
	{
		readonly ISpecification inner;

		public InverseSpecification( [Required]ISpecification inner )
		{
			this.inner = inner;
		}

		public bool IsSatisfiedBy( object context ) => !inner.IsSatisfiedBy( context );
	}
	
	public class NullSpecification : ISpecification<object>
	{
		public static NullSpecification Instance { get; } = new NullSpecification();

		public static InverseSpecification NotNull { get; } = new InverseSpecification( Instance );

		bool ISpecification<object>.IsSatisfiedBy( object parameter ) => IsSatisfiedBy( parameter );

		public bool IsSatisfiedBy( object context ) => context.IsNull();
	}

	public class CheckSpecification<T> : SpecificationBase<T>
	{
		protected override bool Verify( T parameter ) => new Checked( parameter, this ).Value.Apply();
	}

	public abstract class SpecificationBase<TParameter> : ISpecification<TParameter>
	{
		bool ISpecification.IsSatisfiedBy( object parameter ) => parameter.AsTo<TParameter, bool>( IsSatisfiedBy );

		public bool IsSatisfiedBy( TParameter parameter ) => Verify( parameter );

		protected abstract bool Verify( TParameter parameter );
	}
}