using System;
using System.Reflection;
using DragonSpark.Sources.Coercion;

namespace DragonSpark.TypeSystem
{
	public sealed class TypeInfoCoercer : CoercerBase<Type, TypeInfo>
	{
		public static TypeInfoCoercer Default { get; } = new TypeInfoCoercer();
		TypeInfoCoercer() {}

		protected override TypeInfo Coerce( Type parameter ) => parameter.GetTypeInfo();
	}
}