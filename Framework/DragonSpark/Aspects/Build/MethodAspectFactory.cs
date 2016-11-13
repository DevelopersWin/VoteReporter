using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public sealed class MethodAspectFactory<T> : AspectInstanceFactoryBase<MethodInfo> where T : IAspect
	{
		public static MethodAspectFactory<T> Default { get; } = new MethodAspectFactory<T>();
		MethodAspectFactory() : base( ContainsAspectSpecification<T>.Default, AspectInstances<T>.Default ) {}
	}
}