﻿using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Specifications
{
	class SpecificationAwareConstructor : AdapterConstructorBase<ISpecificationAware>
	{
		public static IParameterizedSource<Type, Func<object, ISpecificationAware>> Default { get; } = new SpecificationAwareConstructor().ToCache();
		SpecificationAwareConstructor() : base( typeof(ISpecificationAware<>), typeof(SpecificationAwareAdapter<>) ) {}
	}
}