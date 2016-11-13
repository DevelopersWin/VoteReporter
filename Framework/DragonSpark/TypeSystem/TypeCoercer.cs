using DragonSpark.Sources.Coercion;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class TypeCoercer : CoercerBase<Type>
	{
		public static TypeCoercer Default { get; } = new TypeCoercer();
		TypeCoercer() {}

		protected override Type Coerce( object parameter )
		{
			var aware = parameter as ITypeAware;
			if ( aware != null )
			{
				return aware.ReferencedType;
			}

			var info = parameter as ParameterInfo;
			if ( info != null )
			{
				return info.ParameterType;
			}

			var type = parameter as Type;
			if ( type != null )
			{
				return type;
			}

			var member = parameter as MemberInfo;
			var memberType = member?.GetMemberType();
			var result = memberType ?? parameter.GetType();
			return result;
		}
	}

	public sealed class ObjectTypeCoercer : CoercerBase<Type>
	{
		public static ObjectTypeCoercer Default { get; } = new ObjectTypeCoercer();
		ObjectTypeCoercer() {}

		protected override Type Coerce( object parameter ) => parameter.GetType();
	}

	public sealed class AsTypeCoercer : CoercerBase<TypeInfo, Type>
	{
		public static AsTypeCoercer Default { get; } = new AsTypeCoercer();
		AsTypeCoercer() {}

		protected override Type Coerce( TypeInfo parameter ) => parameter.AsType();
	}
	
	public sealed class TypeInfoCoercer : CoercerBase<Type, TypeInfo>
	{
		public static TypeInfoCoercer Default { get; } = new TypeInfoCoercer();
		TypeInfoCoercer() {}

		protected override TypeInfo Coerce( Type parameter ) => parameter.GetTypeInfo();
	}
}