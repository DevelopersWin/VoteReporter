using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects.Internals;
using System;
using System.Runtime.CompilerServices;

namespace DragonSpark.Application
{
	public sealed class ApplicationTypeSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new ApplicationTypeSpecification().ToCachedSpecification();
		ApplicationTypeSpecification() {}

		public override bool IsSatisfiedBy( Type parameter ) => Defaults.ProvidesInstanceSpecification.IsSatisfiedBy( parameter ) && !typeof(MethodBinding).Adapt().IsAssignableFrom( parameter ) && !parameter.Has<CompilerGeneratedAttribute>();
	}
}