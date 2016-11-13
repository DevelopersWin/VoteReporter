using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public interface IAspectProvider<in T> : IAspectProvider, IParameterizedSource<T, ImmutableArray<AspectInstance>?> where T : MemberInfo {}
}