using System;
using DragonSpark.Specifications;

namespace DragonSpark.Activation.Location
{
	public abstract class LocatorBase : ActivatorBase<LocateTypeRequest>
	{
		readonly protected static Coerce<LocateTypeRequest> DefaultCoerce = Coercer.Default.ToDelegate();

		protected LocatorBase() : base( DefaultCoerce ) {}

		protected LocatorBase( ISpecification<LocateTypeRequest> specification ) : base( DefaultCoerce, specification ) {}

		public sealed class Coercer : TypeRequestCoercer<LocateTypeRequest>
		{
			public static Coercer Default { get; } = new Coercer();
		
			protected override LocateTypeRequest Create( Type type ) => new LocateTypeRequest( type );
		}
	}
}