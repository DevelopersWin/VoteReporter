using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Aspects
{
	public sealed class GenericSpecificationDefinition : SpecificationDefinitionBase
	{
		public static GenericSpecificationDefinition Default { get; } = new GenericSpecificationDefinition();
		GenericSpecificationDefinition() : base( typeof(ISpecification<>) ) {}
	}

	public sealed class GeneralizedSpecificationDefinition : SpecificationDefinitionBase
	{
		public static GeneralizedSpecificationDefinition Default { get; } = new GeneralizedSpecificationDefinition();
		GeneralizedSpecificationDefinition() : base( typeof(ISpecification<object>) ) {}
	}

	public sealed class GeneralizedParameterizedSourceDefinition : MethodDefinitionBase
	{
		public static GeneralizedParameterizedSourceDefinition Default { get; } = new GeneralizedParameterizedSourceDefinition();
		GeneralizedParameterizedSourceDefinition() : base( new MethodStore( typeof(IParameterizedSource<object, object>), nameof(IParameterizedSource<object, object>.Get) ) ) {}
	}

	public abstract class SpecificationDefinitionBase : MethodDefinitionBase
	{
		protected SpecificationDefinitionBase( Type specificationType ) : base( new MethodStore( specificationType, nameof(ISpecification<object>.IsSatisfiedBy) ) ) {}
	}

	public abstract class MethodDefinitionBase : Definition
	{
		protected MethodDefinitionBase( IMethodStore method ) : base( method.DeclaringType, method )
		{
			Method = method;
		}

		public IMethodStore Method { get; }
	}
}