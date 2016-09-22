using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Aspects
{
	public sealed class GenericSpecificationTypeDefinition : SpecificationTypeDefinitionBase
	{
		public static GenericSpecificationTypeDefinition Default { get; } = new GenericSpecificationTypeDefinition();
		GenericSpecificationTypeDefinition() : base( typeof(ISpecification<>) ) {}
	}

	public sealed class GeneralizedSpecificationTypeDefinition : SpecificationTypeDefinitionBase
	{
		public static GeneralizedSpecificationTypeDefinition Default { get; } = new GeneralizedSpecificationTypeDefinition();
		GeneralizedSpecificationTypeDefinition() : base( typeof(ISpecification<object>) ) {}
	}

	public sealed class GeneralizedParameterizedSourceTypeDefinition : TypeDefinitionWithPrimaryMethodBase
	{
		public static GeneralizedParameterizedSourceTypeDefinition Default { get; } = new GeneralizedParameterizedSourceTypeDefinition();
		GeneralizedParameterizedSourceTypeDefinition() : base( new MethodStore( typeof(IParameterizedSource<object, object>), nameof(IParameterizedSource<object, object>.Get) ) ) {}
	}

	public abstract class SpecificationTypeDefinitionBase : TypeDefinitionWithPrimaryMethodBase
	{
		protected SpecificationTypeDefinitionBase( Type specificationType ) : base( new MethodStore( specificationType, nameof(ISpecification<object>.IsSatisfiedBy) ) ) {}
	}

	public abstract class TypeDefinitionWithPrimaryMethodBase : TypeDefinition
	{
		protected TypeDefinitionWithPrimaryMethodBase( IMethodStore method ) : base( method.DeclaringType, method )
		{
			Method = method;
		}

		public IMethodStore Method { get; }
	}
}