using DragonSpark.Activation.Location;
using DragonSpark.Aspects.Alteration;
using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using Ploeh.AutoFixture.Kernel;
using System;
using System.Collections.Generic;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	[ApplyAutoValidation, ApplySpecification( typeof(ContainsSingletonPropertySpecification) ), ApplyResultAlteration( typeof(DefaultItemValueAlteration<IMethod>) )]
	public sealed class SingletonQuery : ParameterizedSourceBase<Type, IEnumerable<IMethod>>, IMethodQuery, ISpecification<Type>
	{
		public static SingletonQuery Default { get; } = new SingletonQuery();
		SingletonQuery() {}

		public override IEnumerable<IMethod> Get( Type parameter )
		{
			yield return new SingletonMethod( parameter );
		}

		IEnumerable<IMethod> IMethodQuery.SelectMethods( Type type ) => Get( type );

		bool ISpecification<Type>.IsSatisfiedBy( Type parameter ) => false; // TODO: Undo/replace.
	}
}