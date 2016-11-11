using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public interface IAspectProvider<in T> : IAspectProvider, IParameterizedSource<T, ImmutableArray<AspectInstance>> where T : MemberInfo {}

	public class CompositeAspectProvider<T> : CompositeFactory<T, AspectInstance>, IAspectProvider<T> where T : MemberInfo
	{
		public CompositeAspectProvider( params IParameterizedSource<T, AspectInstance>[] sources ) : base( sources ) {}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => this.GetEnumerable( (T)targetElement );
	}
}
