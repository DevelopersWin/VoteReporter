using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public sealed class AspectInstanceFactory<T> : SpecificationParameterizedSource<MethodInfo, AspectInstance>
	{
		public static AspectInstanceFactory<T> Default { get; } = new AspectInstanceFactory<T>();
		AspectInstanceFactory() : base( HasAspectSpecification.DefaultNested.Inverse(), Factory.DefaultNested.Get ) {}
		
		sealed class HasAspectSpecification : SpecificationBase<MethodInfo>
		{
			readonly static Type AspectType = typeof(T);

			public static HasAspectSpecification DefaultNested { get; } = new HasAspectSpecification();
			HasAspectSpecification() : this( () => PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>() ) {}

			readonly Func<IAspectRepositoryService> repositorySource;

			HasAspectSpecification( Func<IAspectRepositoryService> repositorySource )
			{
				this.repositorySource = repositorySource;
			}

			public override bool IsSatisfiedBy( MethodInfo parameter ) => repositorySource().HasAspect( parameter, AspectType );
		}

		sealed class Factory : ParameterizedSourceBase<MethodInfo, AspectInstance>
		{
			public static Factory DefaultNested { get; } = new Factory();
			Factory() : this( new ObjectConstruction( typeof(T), Items<object>.Default ) ) {}

			readonly ObjectConstruction construction;

			Factory( ObjectConstruction construction )
			{
				this.construction = construction;
			}

			public override AspectInstance Get( MethodInfo parameter ) => new AspectInstance( parameter, construction, null );
		}
	}
}