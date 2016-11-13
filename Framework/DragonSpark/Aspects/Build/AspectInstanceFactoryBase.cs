using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Reflection;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class AspectInstances : CacheWithImplementedFactoryBase<MemberInfo, AspectInstance>
	{
		readonly ObjectConstruction construction;

		public AspectInstances( ObjectConstruction construction )
		{
			this.construction = construction;
		}

		protected override AspectInstance Create( MemberInfo parameter ) => new AspectInstance( parameter, construction, null );
	}

	/*public sealed class ContainsAspectSpecification<T> : DelegatedSpecification<MemberInfo> where T : IAspect
	{
		public static ContainsAspectSpecification<T> Default { get; } = new ContainsAspectSpecification<T>();
		ContainsAspectSpecification() : base( ContainsAspectSpecification.Delegates.Get( typeof(T) ) ) {}
	}*/

	public sealed class CanApplyAspectSpecification : AllSpecification<MemberInfo>
	{
		public CanApplyAspectSpecification( [OfType( typeof(IAspect) )]Type aspectType, ICache<MemberInfo, AspectInstance> source )
			: base( ContainsAspectSpecification.Default.Get( aspectType ).Inverse(), new DelegatedSpecification<MemberInfo>( source.Contains ).Inverse() ) {}
	}

	public abstract class AspectInstanceFactoryBase<T> : SpecificationParameterizedSource<T, AspectInstance> where T : MemberInfo
	{
		protected AspectInstanceFactoryBase( ObjectConstruction construction, [OfType( typeof(IAspect) )]Type aspectType ) : this( new AspectInstances( construction ), aspectType ) {}

		[UsedImplicitly]
		protected AspectInstanceFactoryBase( ICache<MemberInfo, AspectInstance> source, [OfType( typeof(IAspect) )]Type aspectType ) 
			: this( new CanApplyAspectSpecification( aspectType, source ), source.Get ) {}

		[UsedImplicitly]
		protected AspectInstanceFactoryBase( ISpecification<T> specification, Func<T, AspectInstance> source ) : base( specification, source ) {}
	}
}