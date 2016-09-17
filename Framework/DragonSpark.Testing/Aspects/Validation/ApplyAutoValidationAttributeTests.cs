using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects.Validation
{
	public class ApplyAutoValidationAttributeTests : TestCollectionBase
	{
		readonly ExtendedFactory factory = new ExtendedFactory();
		readonly AutoValidatingSource<int, float> validating;
		readonly AppliedExtendedFactory applied = new AppliedExtendedFactory();

		public ApplyAutoValidationAttributeTests( ITestOutputHelper output ) : base( output )
		{
			validating = new AutoValidatingSource<int, float>( factory, factory );
		}

		[Fact]
		[Trait( Traits.Category, Traits.Categories.Performance )]
		/*
		Test                                | Average |  Median |    Mode
		-----------------------------------------------------------------
		BasicAutoValidation                 | 00.0213 | 00.0211 | 00.0210
		BasicAutoValidationWithAspect       | 00.0327 | 00.0326 | 00.0325
		BasicAutoValidationInline           | 00.0353 | 00.0351 | 00.0350
		BasicAutoValidationInlineWithAspect | 00.0689 | 00.0687 | 00.0691
		*/
		public void Performance()
		{
			new PerformanceSupport( WriteLine, BasicAutoValidation, BasicAutoValidationWithAspect, BasicAutoValidationInline, BasicAutoValidationInlineWithAspect ).Run();
		}

		[Fact]
		public void BasicAutoValidation() => BasicAutoValidationWith( validating, validating, factory );

		[Fact]
		public void BasicAutoValidationInline()
		{
			var sut = new ExtendedFactory();
			var source = new AutoValidatingSource<int, float>( sut, sut );
			BasicAutoValidationWith( source, source, sut );
		}

		[Fact]
		public void BasicAutoValidationWithAspect() => BasicAutoValidationWith( applied, applied, applied );

		[Fact]
		public void BasicAutoValidationInlineWithAspect()
		{
			var sut = new AppliedExtendedFactory();
			BasicAutoValidationWith( sut, sut, sut );
		}

		[Fact]
		public void ParameterHandler()
		{
			var sut = new CachedAppliedExtendedFactory();
			var first = sut.Get( 6776 );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, first );

			var can = sut.IsSatisfiedBy( 6776 );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.True( can );

			var second = sut.Get( 6776 );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( first, second );
		}

		static void BasicAutoValidationWith( IParameterizedSource<int, float> factory, ISpecification<int> specification, IExtendedFactory sut )
		{
			Assert.Equal( 0, sut.CanCreateGenericCalled );

			var cannot = specification.IsSatisfiedBy( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			factory.Get( 123 );

			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var can = specification.IsSatisfiedBy( 6776 );
			Assert.True( can );
			Assert.Equal( 3, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = factory.Get( 6776 );
			Assert.Equal( 3, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			sut.Reset();
		}

		interface IExtendedFactory : IParameterizedSource<int, float>, ISpecification<int>
		{
			int CanCreateGenericCalled { get; }

			int CreateGenericCalled { get; }

			void Reset();
		}

		class CachedAppliedExtendedFactory : AppliedExtendedFactory
		{
			[Freeze]
			public override float Get( int parameter ) => base.Get( parameter );
		}

		[ApplyAutoValidation]
		class AppliedExtendedFactory : IExtendedFactory
		{
			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => CanCreateGenericCalled = CreateGenericCalled = 0;

			public bool IsSatisfiedBy( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public virtual float Get( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}

		class ExtendedFactory : IExtendedFactory
		{
			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => CanCreateGenericCalled = CreateGenericCalled = 0;

			public bool IsSatisfiedBy( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public float Get( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}
	}
}