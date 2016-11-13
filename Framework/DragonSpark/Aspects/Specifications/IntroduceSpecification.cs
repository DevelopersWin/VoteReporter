using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceSpecification : IntroduceGenericInterfaceAspectBase
	{
		readonly static Func<Type, Func<object, object>> Factory = new ImplementationCache( typeof(ISpecificationAdapter) ).ToCache().ToDelegate();

		public IntroduceSpecification() : this( typeof(DefaultSpecificationImplementation<>) ) {}
		public IntroduceSpecification( Type implementationType ) : this( implementationType, SourceCoercer<ISpecificationAdapter>.Default.To( Factory( implementationType ) ).Get ) {}
		public IntroduceSpecification( Type implementationType, Func<object, object> factory ) : base( SpecificationTypeDefinition.Default, implementationType, factory ) {}
	}

	public sealed class DefaultSpecificationImplementation<T> : SpecificationBase<T>
	{
		readonly ISpecificationAdapter specification;

		public DefaultSpecificationImplementation( ISpecificationAdapter specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( T parameter ) => (bool)specification.Get( parameter );
	}

}