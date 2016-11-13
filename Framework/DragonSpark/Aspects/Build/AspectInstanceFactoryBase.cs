using DragonSpark.Sources.Coercion;
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
	public sealed class AspectInstances<T> : AspectInstances where T : IAspect
	{
		public static AspectInstances<T> Default { get; } = new AspectInstances<T>();
		AspectInstances() : base( ObjectConstructionFactory<T>.Default.Get() ) {}

		/*public AspectInstances( ObjectConstruction construction ) : base( construction ) {}*/
	}

	public class AspectInstances : CacheWithImplementedFactoryBase<MemberInfo, AspectInstance>
	{
		readonly ObjectConstruction construction;

		public AspectInstances( ObjectConstruction construction )
		{
			this.construction = construction;
		}

		protected override AspectInstance Create( MemberInfo parameter ) => new AspectInstance( parameter, construction, null );
	}

	public sealed class ContainsAspectSpecification : SpecificationCache<Type, MemberInfo>
	{
		public static ContainsAspectSpecification Default { get; } = new ContainsAspectSpecification();
		public static IParameterizedSource<Type, Func<MemberInfo, bool>> Delegates { get; } = Default.To( DelegateCoercer.Default ).ToCache();
		ContainsAspectSpecification() : base( type => new DefaultImplementation( type ) ) {}

		public sealed class DefaultImplementation : SpecificationBase<MemberInfo>
		{
			readonly IAspectRepositoryService repositorySource;
			readonly Type aspectType;

			public DefaultImplementation( [OfType( typeof(IAspect) )] Type aspectType ) : this( AspectRepositoryService.Default, aspectType ) {}

			[UsedImplicitly]
			public DefaultImplementation( IAspectRepositoryService repositorySource, [OfType( typeof(IAspect) )] Type aspectType )
			{
				this.repositorySource = repositorySource;
				this.aspectType = aspectType;
			}

			public override bool IsSatisfiedBy( MemberInfo parameter ) => repositorySource.HasAspect( parameter, aspectType );
		}
	}

	/*public sealed class ContainsAspectSpecification<T> : DelegatedSpecification<MemberInfo> where T : IAspect
	{
		public static ContainsAspectSpecification<T> Default { get; } = new ContainsAspectSpecification<T>();
		ContainsAspectSpecification() : base( ContainsAspectSpecification.Delegates.Get( typeof(T) ) ) {}
	}*/

	public sealed class CanApplyAspectSpecification<T> : DelegatedSpecification<MemberInfo> where T : IAspect
	{
		public static CanApplyAspectSpecification<T> Default { get; } = new CanApplyAspectSpecification<T>();
		CanApplyAspectSpecification() : this( AspectInstances<T>.Default ) {}

		public CanApplyAspectSpecification( ICache<MemberInfo, AspectInstance> source ) : base( new CanApplyAspectSpecification( typeof(T), source ).IsSatisfiedBy ) {}
	}

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