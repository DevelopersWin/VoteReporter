using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public sealed class MethodAspectFactory<T> : AspectInstanceFactoryBase<MethodInfo> where T : IMethodLevelAspect
	{
		public static MethodAspectFactory<T> Default { get; } = new MethodAspectFactory<T>();
		MethodAspectFactory() : base( CanApplyAspectSpecification<T>.Default, AspectInstances<T>.Default.Get ) {}
	}
}