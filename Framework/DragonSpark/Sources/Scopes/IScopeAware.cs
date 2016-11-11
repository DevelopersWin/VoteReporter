using System;

namespace DragonSpark.Sources.Scopes
{
	public interface IScopeAware : IAssignable<ISourceAware> {}

	public interface IScopeAware<in T> : IScopeAware, IAssignable<Func<object, T>>, IAssignable<Func<T>> {}
}