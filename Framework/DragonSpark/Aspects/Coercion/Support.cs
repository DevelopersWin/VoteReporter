﻿using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Support : AspectBuildDefinition<Aspect>
	{
		public static Support Default { get; } = new Support();
		Support() : base( CommandTypeDefinition.Default, GeneralizedSpecificationTypeDefinition.Default, GeneralizedParameterizedSourceTypeDefinition.Default ) {}
	}
}