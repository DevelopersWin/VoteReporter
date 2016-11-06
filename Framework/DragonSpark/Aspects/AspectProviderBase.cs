using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	public abstract class AspectProviderBase<T> : ParameterizedItemSourceBase<T, AspectInstance>, IAspectProvider
	{
		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => this.GetEnumerable( (T)targetElement );
	}
}
