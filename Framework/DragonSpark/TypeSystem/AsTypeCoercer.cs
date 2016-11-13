using System;
using System.Reflection;
using DragonSpark.Sources.Coercion;

namespace DragonSpark.TypeSystem
{
	public sealed class AsTypeCoercer : CoercerBase<TypeInfo, Type>
	{
		public static AsTypeCoercer Default { get; } = new AsTypeCoercer();
		AsTypeCoercer() {}

		protected override Type Coerce( TypeInfo parameter ) => parameter.AsType();
	}
}