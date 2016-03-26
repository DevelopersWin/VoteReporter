using System;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;

namespace DragonSpark.Activation
{
	public abstract class ConstructorBase<T> : ActivatorBase<ConstructTypeRequest, T> where T : class
	{
		protected ConstructorBase() : base( Coercer.Instance ) {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification  ) : base( specification, Coercer.Instance ) {}

		public class Coercer : TypeRequestCoercer<ConstructTypeRequest, T>
		{
			public new static Coercer Instance { get; } = new Coercer();

			protected override ConstructTypeRequest Create( Type type, object parameter ) => new ConstructTypeRequest( type, parameter?.ToItem() ?? Default<object>.Items );
		}
	}

	public abstract class ConstructorBase : ConstructorBase<object>
	{
		protected ConstructorBase() {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification ) : base( specification ) {}
	}
}