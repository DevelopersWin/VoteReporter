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

	public sealed class ContainsAspectSpecification<T> : DelegatedSpecification<MemberInfo> where T : IAspect
	{
		public static ContainsAspectSpecification<T> Default { get; } = new ContainsAspectSpecification<T>();
		ContainsAspectSpecification() : base( ContainsAspectSpecification.Delegates.Get( typeof(T) ) ) {}
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

	public abstract class AspectInstanceFactoryBase<T> : SpecificationParameterizedSource<T, AspectInstance> where T : MemberInfo
	{
		protected AspectInstanceFactoryBase( ObjectConstruction construction, [OfType( typeof(IAspect) )]Type aspectType )
			: this( ContainsAspectSpecification.Default.Get( aspectType ), new AspectInstances( construction ) ) {}

		[UsedImplicitly]
		protected AspectInstanceFactoryBase( ISpecification<MemberInfo> specification, ICache<T, AspectInstance> source ) 
			: base( specification.Or( new DelegatedSpecification<T>( source.Contains ) ).Inverse(), source.Get ) {}
	}
}