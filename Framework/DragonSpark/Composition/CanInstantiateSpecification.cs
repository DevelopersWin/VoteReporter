using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class CanInstantiateSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new CanInstantiateSpecification().ToCachedSpecification();
		CanInstantiateSpecification() {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = !info.IsGenericTypeDefinition && !info.ContainsGenericParameters && !info.IsInterface && !info.IsAbstract && info.DeclaredConstructors.Any( constructorInfo => constructorInfo.IsPublic ) && ( info.IsPublic || info.Assembly.Has<RegistrationAttribute>() );
			return result;
		}
	}
}