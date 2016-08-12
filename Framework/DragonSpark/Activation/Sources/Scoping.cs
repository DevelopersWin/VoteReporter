using System;

namespace DragonSpark.Activation.Sources
{
	public interface IScopeAware<in T> : IScopeAware, IAssignable<Func<object, T>>, IAssignable<Func<T>> {}
}