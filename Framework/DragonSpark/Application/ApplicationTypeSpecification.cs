using System;
using System.Runtime.CompilerServices;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects.Internals;

namespace DragonSpark.Application
{
	public class ApplicationTypeSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new ApplicationTypeSpecification().ToCachedSpecification();
		ApplicationTypeSpecification() {}

		public override bool IsSatisfiedBy( Type parameter ) => Defaults.ActivateSpecification.IsSatisfiedBy( parameter ) && !typeof(MethodBinding).Adapt().IsAssignableFrom( parameter ) && !parameter.Has<CompilerGeneratedAttribute>();
	}
}