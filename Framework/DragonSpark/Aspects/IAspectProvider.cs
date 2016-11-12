using System.Collections.Immutable;
using System.Reflection;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects
{
	public interface IAspectProvider<in T> : IAspectProvider, IParameterizedSource<T, ImmutableArray<AspectInstance>> where T : MemberInfo {}
}