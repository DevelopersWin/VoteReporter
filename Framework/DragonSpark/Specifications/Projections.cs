using System;
using System.Reflection;

namespace DragonSpark.Specifications
{
	public static class Projections
	{
		public static Func<Type, MemberInfo> MemberType { get; } = info => info.GetTypeInfo();
	}
}