using DragonSpark.Sources.Coercion;
using System;

namespace DragonSpark.TypeSystem
{
	public sealed class ObjectTypeCoercer : CoercerBase<Type>
	{
		public static ObjectTypeCoercer Default { get; } = new ObjectTypeCoercer();
		ObjectTypeCoercer() {}

		protected override Type Coerce( object parameter ) => parameter as Type ?? parameter.GetType();
	}
}