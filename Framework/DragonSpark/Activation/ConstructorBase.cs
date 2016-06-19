using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Activation
{
	public abstract class ConstructorBase : ActivatorBase<ConstructTypeRequest>
	{
		readonly protected static Coerce<ConstructTypeRequest> Coerce = Coercer.Instance.ToDelegate();

		protected ConstructorBase() : base( Coerce ) {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification  ) : base( Coerce, specification ) {}

		public class Coercer : TypeRequestCoercer<ConstructTypeRequest>
		{
			public static Coercer Instance { get; } = new Coercer();

			protected override ConstructTypeRequest Create( Type type ) => new ConstructTypeRequest( type );
		}
	}

	/*public abstract class ConstructorBase : ConstructorBase<object>
	{
		protected ConstructorBase() {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification ) : base( specification ) {}
	}*/
}