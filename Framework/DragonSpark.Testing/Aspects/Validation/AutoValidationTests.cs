using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using PostSharp.Patterns.Model;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects.Validation
{
	public class AutoValidationTests : TestCollectionBase
	{
		[Reference]
		readonly Factory factory = new Factory();
		[Reference]
		readonly AutoValidatingFactory validating;
		[Reference]
		readonly AppliedFactory applied = new AppliedFactory();

		public AutoValidationTests( ITestOutputHelper output ) : base( output )
		{
			validating = new AutoValidatingFactory( factory );
		}

		[Fact]
		[Trait( Traits.Category, Traits.Categories.Performance )]
		public void Performance()
		{
			new PerformanceSupport( WriteLine, BasicAutoValidation, BasicAutoValidationInline, BasicAutoValidationApplied, BasicAutoValidationAppliedInline ).Run();
		}

		[Fact]
		public void BasicAutoValidation() => BasicAutoValidationWith( validating, factory );

		[Fact]
		public void BasicAutoValidationApplied() => BasicAutoValidationWith( applied, applied );

		[Fact]
		public void BasicAutoValidationInline()
		{
			var sut = new Factory();
			BasicAutoValidationWith( new AutoValidatingFactory( sut ), sut );
		}

		[Fact]
		public void BasicAutoValidationAppliedInline()
		{
			var sut = new AppliedFactory();
			BasicAutoValidationWith( sut, sut );
		}

		static void BasicAutoValidationWith( IFactoryWithParameter factory, IFactory sut )
		{
			var cannot = factory.CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );

			for ( int i = 0; i < 10; i++ )
			{
				var can = factory.CanCreate( 123 );
				Assert.True( can );
			}

			Assert.Equal( 11, sut.CanCreateCalled );

			Assert.Equal( 0, sut.CreateCalled );

			var created = factory.Create( 123 );
			Assert.Equal( 11, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 6776, created );
			sut.Reset();
		}

		[ApplyAutoValidation]
		public class AppliedFactory : IFactory
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return (int)parameter == 123;
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return 6776;
			}

			public void Reset()
			{
				CanCreateCalled = CreateCalled = 0;
			}
		}

		public class Factory : IFactory
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return (int)parameter == 123;
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return 6776;
			}

			public void Reset()
			{
				CanCreateCalled = CreateCalled = 0;
			}
		}
	}
}