using System;

namespace DragonSpark.Runtime
{
	public struct MethodDescriptor
	{
		public MethodDescriptor( Type declaringType, string methodName )
		{
			DeclaringType = declaringType;
			MethodName = methodName;
		}

		public Type DeclaringType { get; }

		public string MethodName { get; }
	}
}