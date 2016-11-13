using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public sealed class TypeAspectFactory<T> : AspectInstanceFactoryBase<TypeInfo> where T : IAspect
	{
		public static TypeAspectFactory<T> Default { get; } = new TypeAspectFactory<T>();
		TypeAspectFactory() : base( ContainsAspectSpecification<T>.Default, AspectInstances<T>.Default ) {}
	}
}