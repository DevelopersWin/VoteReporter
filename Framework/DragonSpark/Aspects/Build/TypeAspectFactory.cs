using System.Reflection;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public sealed class TypeAspectFactory<T> : AspectInstanceFactoryBase<TypeInfo, T> where T : IAspect
	{
		public static TypeAspectFactory<T> Default { get; } = new TypeAspectFactory<T>();
		TypeAspectFactory() {}
	}
}