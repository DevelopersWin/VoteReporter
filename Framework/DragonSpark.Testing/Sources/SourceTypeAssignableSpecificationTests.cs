using DragonSpark.Activation;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Scopes;
using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using Xunit;

namespace DragonSpark.Testing.Sources
{
	public class SourceTypeAssignableSpecificationTests
	{
		[Fact]
		public void VerifyCaching()
		{
			var source = SourceAccountedTypes.Default.To( ParameterConstructor<ImmutableArray<Type>, CompositeAssignableSpecification>.Default ).ToSingleton();
			Assert.Same( source( GetType() ), source( GetType() ) );
		}
	}
}