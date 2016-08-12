using System;

namespace DragonSpark.Runtime.Sources
{
	public interface IScopeAware<in T> : IScopeAware, IAssignable<Func<object, T>>, IAssignable<Func<T>> {}
}