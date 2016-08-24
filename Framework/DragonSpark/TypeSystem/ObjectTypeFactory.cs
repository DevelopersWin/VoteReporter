using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.TypeSystem
{
	sealed class ObjectTypeFactory : ParameterizedSourceBase<object[], Type[]>
	{
		public static ObjectTypeFactory Default { get; } = new ObjectTypeFactory();

		public override Type[] Get( object[] parameter )
		{
			var result = new Type[parameter.Length];
			for ( var i = 0; i < parameter.Length; i++ )
			{
				result[i] = parameter[i]?.GetType();
			}
			return result;
		}
	}
}