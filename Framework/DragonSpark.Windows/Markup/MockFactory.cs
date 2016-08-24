using System;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using Moq;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Windows.Markup
{
	public class MockFactory : ValidatedParameterizedSourceBase<Type, object>
	{
		public static MockFactory Default { get; } = new MockFactory();

		MockFactory() : base( Specification.DefaultNested ) {}

		class Specification : SpecificationBase<Type>
		{
			public static Specification DefaultNested { get; } = new Specification();

			public override bool IsSatisfiedBy( Type parameter ) => parameter.IsInterface || !parameter.IsSealed;
		}

		public override object Get( Type parameter )
		{
			var type = typeof(Mock<>).MakeGenericType( parameter );
			var result = Activator.Activate<Mock>( type ).Object;
			return result;
		}
	}
}