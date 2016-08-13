using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using PostSharp.Patterns.Model;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects.Validation
{
	public class GenericAutoValidationTests : TestCollectionBase
	{
		[Reference]
		readonly ExtendedFactory factory = new ExtendedFactory();

		[Reference]
		readonly IValidatedParameterizedSource<int, float> validating;
		[Reference]
		readonly AppliedExtendedFactory applied = new AppliedExtendedFactory();

		public GenericAutoValidationTests( ITestOutputHelper output ) : base( output )
		{
			validating = new AutoValidatingSource<int, float>( factory );
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
		public void BasicAutoValidationInline()
		{
			var sut = new ExtendedFactory();
			BasicAutoValidationWith( new AutoValidatingSource<int, float>( sut ), sut );
		}

		[Fact]
		public void BasicAutoValidationApplied() => BasicAutoValidationWith( applied, applied );

		[Fact]
		public void BasicAutoValidationAppliedInline()
		{
			var sut = new AppliedExtendedFactory();
			BasicAutoValidationWith( sut, sut );
		}

		[Fact]
		public void ParameterHandler()
		{
			var sut = new CachedAppliedExtendedFactory();
			var first = sut.Get( 6776 );
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, first );

			var can = sut.IsValid( 6776 );
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.True( can );

			var second = sut.Get( 6776 );
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( first, second );
		}

		static void BasicAutoValidationWith( IValidatedParameterizedSource<int, float> factory, IExtendedFactory sut )
		{
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );

			var invalid = factory.IsValid( "Message" );
			Assert.False( invalid );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );
			
			var cannot = factory.IsValid( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );

			var can = factory.IsValid( 6776 );
			Assert.True( can );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = factory.Get( 6776 );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			sut.Reset();
		}

		

		interface IExtendedFactory : IValidatedParameterizedSource<int, float>
		{
			int CanCreateCalled { get; }

			int CreateCalled { get; }

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
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => CanCreateCalled = CreateCalled = CanCreateGenericCalled = CreateGenericCalled = 0;

			public bool IsValid( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && IsValid( (int)parameter );
			}

			object IParameterizedSource.Get( object parameter )
			{
				CreateCalled++;
				return Get( (int)parameter );
			}

			public bool IsValid( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public virtual float Get( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}

			public bool IsSatisfiedBy( int parameter ) => IsValid( parameter );

			public bool IsSatisfiedBy( object parameter ) => IsValid( parameter );
		}

		class ExtendedFactory : IExtendedFactory
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => CanCreateCalled = CreateCalled = CanCreateGenericCalled = CreateGenericCalled = 0;

			public bool IsValid( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && IsValid( (int)parameter );
			}

			public object Get( object parameter )
			{
				CreateCalled++;
				return Get( (int)parameter );
			}

			public bool IsValid( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public float Get( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}

			public bool IsSatisfiedBy( int parameter ) => IsValid( parameter );

			public bool IsSatisfiedBy( object parameter ) => IsValid( parameter );
		}
	}
}