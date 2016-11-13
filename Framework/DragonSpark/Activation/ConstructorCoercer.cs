using System;
using DragonSpark.Sources.Coercion;

namespace DragonSpark.Activation
{
	public sealed class ConstructorCoercer : CoercerBase<Type, ConstructTypeRequest>
	{
		public static ConstructorCoercer Default { get; } = new ConstructorCoercer();
		ConstructorCoercer() {}

		protected override ConstructTypeRequest Coerce( Type parameter ) => new ConstructTypeRequest( parameter );
	}
}