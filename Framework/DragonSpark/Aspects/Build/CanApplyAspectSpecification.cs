using System.Reflection;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public sealed class CanApplyAspectSpecification<T> : DelegatedSpecification<MemberInfo> where T : IAspect
	{
		public static CanApplyAspectSpecification<T> Default { get; } = new CanApplyAspectSpecification<T>();
		CanApplyAspectSpecification() : this( AspectInstances<T>.Default ) {}

		public CanApplyAspectSpecification( ICache<MemberInfo, AspectInstance> source ) : base( new CanApplyAspectSpecification( typeof(T), source ).IsSatisfiedBy ) {}
	}
}