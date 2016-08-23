using System;
using DragonSpark.Specifications;

namespace DragonSpark.Activation
{
	public abstract class ConstructorBase : ActivatorBase<ConstructTypeRequest>
	{
		readonly protected static Coerce<ConstructTypeRequest> DefaultCoerce = Coercer.Default.ToDelegate();

		protected ConstructorBase() : base( DefaultCoerce ) {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification  ) : base( DefaultCoerce, specification ) {}

		public sealed class Coercer : TypeRequestCoercer<ConstructTypeRequest>
		{
			public static Coercer Default { get; } = new Coercer();

			protected override ConstructTypeRequest Create( Type type ) => new ConstructTypeRequest( type );
		}
	}

	/*public abstract class ConstructorBase : ConstructorBase<object>
	{
		protected ConstructorBase() {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification ) : base( specification ) {}
	}*/
}