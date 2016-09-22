using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using System;

namespace DragonSpark.Aspects.Build
{
	public sealed class AspectInstanceFactory<TAspect, TTarget> : SpecificationParameterizedSource<TTarget, AspectInstance>
	{
		public static AspectInstanceFactory<TAspect, TTarget> Default { get; } = new AspectInstanceFactory<TAspect, TTarget>();
		AspectInstanceFactory() : base( HasAspectSpecification.DefaultNested.Inverse(), Factory.DefaultNested.Get ) {}
		
		sealed class HasAspectSpecification : SpecificationBase<TTarget>
		{
			readonly static Type AspectType = typeof(TAspect);

			public static HasAspectSpecification DefaultNested { get; } = new HasAspectSpecification();
			HasAspectSpecification() : this( () => PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>() ) {}

			readonly Func<IAspectRepositoryService> repositorySource;

			HasAspectSpecification( Func<IAspectRepositoryService> repositorySource )
			{
				this.repositorySource = repositorySource;
			}

			public override bool IsSatisfiedBy( TTarget parameter ) => repositorySource().HasAspect( parameter, AspectType );
		}

		sealed class Factory : ParameterizedSourceBase<TTarget, AspectInstance>
		{
			public static Factory DefaultNested { get; } = new Factory();
			Factory() : this( new ObjectConstruction( typeof(TAspect), Items<object>.Default ) ) {}

			readonly ObjectConstruction construction;

			Factory( ObjectConstruction construction )
			{
				this.construction = construction;
			}

			public override AspectInstance Get( TTarget parameter ) => new AspectInstance( parameter, construction, null );
		}
	}
}