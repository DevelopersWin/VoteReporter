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
	public abstract class AspectInstanceFactoryBase<TMemberInfo, TAspect> : SpecificationParameterizedSource<TMemberInfo, AspectInstance> 
		where TMemberInfo : MemberInfo
		where TAspect : IAspect
	{
		protected AspectInstanceFactoryBase() : this( Factory.Implementation.ToCache() ) {}

		[UsedImplicitly]
		protected AspectInstanceFactoryBase( ICache<TMemberInfo, AspectInstance> source ) : base( HasAspectSpecification.Implementation.Or( new DelegatedSpecification<TMemberInfo>( source.Contains ) ).Inverse(), source.Get ) {}
		
		sealed class HasAspectSpecification : SpecificationBase<TMemberInfo>
		{
			readonly static Type AspectType = typeof(TAspect);

			public static HasAspectSpecification Implementation { get; } = new HasAspectSpecification();
			HasAspectSpecification() : this( ServiceSource<IAspectRepositoryService>.Default.Get ) {}

			readonly Func<IAspectRepositoryService> repositorySource;

			HasAspectSpecification( Func<IAspectRepositoryService> repositorySource )
			{
				this.repositorySource = repositorySource;
			}

			public override bool IsSatisfiedBy( TMemberInfo parameter ) => repositorySource().HasAspect( parameter, AspectType );
		}

		sealed class Factory : ParameterizedSourceBase<TMemberInfo, AspectInstance>
		{
			public static Factory Implementation { get; } = new Factory();
			Factory() : this( ObjectConstructionFactory<TAspect>.Default.Get() ) {}

			readonly ObjectConstruction construction;

			Factory( ObjectConstruction construction )
			{
				this.construction = construction;
			}

			public override AspectInstance Get( TMemberInfo parameter ) => new AspectInstance( parameter, construction, null );
		}
	}
}