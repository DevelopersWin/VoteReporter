using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public interface IAspectProvider<in T> : IAspectProvider, IParameterizedSource<T, ImmutableArray<AspectInstance>> where T : MemberInfo {}

	public class SourcedAspectProvider<T> : SourcedItemParameterizedSource<T, AspectInstance>, IAspectProvider<T> where T : MemberInfo
	{
		public SourcedAspectProvider( params IParameterizedSource<T, AspectInstance>[] sources ) : base( sources ) {}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => this.GetEnumerable( (T)targetElement );
	}
}
