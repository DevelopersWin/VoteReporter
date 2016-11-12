using System.Reflection;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public sealed class MethodAspectFactory<T> : AspectInstanceFactoryBase<MethodInfo, T> where T : IAspect
	{
		public static MethodAspectFactory<T> Default { get; } = new MethodAspectFactory<T>();
		MethodAspectFactory() {}
	}
}