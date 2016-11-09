using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public abstract class AspectProviderBase<T> : SourcedItemParameterizedSource<T, AspectInstance>, IAspectProvider where T : MemberInfo
	{
		protected AspectProviderBase( params IParameterizedSource<T, AspectInstance>[] sources ) : base( sources ) {}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => this.GetEnumerable( (T)targetElement );
	}
}
