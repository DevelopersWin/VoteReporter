using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Activation
{
	public abstract class ConstructorBase : ActivatorBase<ConstructTypeRequest>
	{
		protected ConstructorBase() : base( Coercer.Instance ) {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification  ) : base( specification, Coercer.Instance ) {}

		public class Coercer : TypeRequestCoercer<ConstructTypeRequest>
		{
			public new static Coercer Instance { get; } = new Coercer();

			protected override ConstructTypeRequest Create( Type type ) => new ConstructTypeRequest( type );
		}
	}

	/*public abstract class ConstructorBase : ConstructorBase<object>
	{
		protected ConstructorBase() {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification ) : base( specification ) {}
	}*/
}