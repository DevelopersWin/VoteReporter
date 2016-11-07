using System;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Relay
{
	public interface IRelayAspectSource : ISpecification<Type>, IParameterizedSource<Type, AspectInstance>, ITypeAware { }
}