using System;
using System.Reflection;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
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
}