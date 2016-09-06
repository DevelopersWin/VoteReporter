using DragonSpark.Aspects;
using DragonSpark.Aspects.Invocation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using PostSharp.Extensibility;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects.Validation
{
	public class ApplyAutoValidationAttributeTests : TestCollectionBase
	{
		//[Reference]
		readonly ExtendedFactory factory = new ExtendedFactory();

		// [Reference]
		readonly AutoValidatingSource<int, float> validating;
		// [Reference]
		readonly AppliedExtendedFactory applied = new AppliedExtendedFactory();

		public ApplyAutoValidationAttributeTests( ITestOutputHelper output ) : base( output )
		{
			validating = new AutoValidatingSource<int, float>( factory, factory );
		}

		[Fact]
		[Trait( Traits.Category, Traits.Categories.Performance )]
		/*
		Test                             | Average |  Median |    Mode
		--------------------------------------------------------------
		BasicAutoValidation              | 00.0326 | 00.0325 | 00.0328
		BasicAutoValidationInline        | 00.0539 | 00.0537 | 00.0535
		BasicAutoValidationApplied       | 00.0410 | 00.0408 | 00.0412
		BasicAutoValidationAppliedInline | 00.0951 | 00.0949 | 00.0941
		*/
		public void Performance()
		{
			new PerformanceSupport( WriteLine, BasicAutoValidation, BasicAutoValidationInline, BasicAutoValidationApplied, BasicAutoValidationAppliedInline ).Run( 1 );
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
		public void BasicAutoValidationApplied() => BasicAutoValidationWith( applied, applied, applied );

		[Fact]
		public void BasicAutoValidationAppliedInline()
		{
			var sut = new AppliedExtendedFactory();
			BasicAutoValidationWith( sut, sut, sut );
		}

		[Fact]
		public void ParameterHandler()
		{
			var sut = new CachedAppliedExtendedFactory();
			var first = sut.Get( 6776 );
			// Assert.Equal( 0, sut.CanCreateCalled );
			// Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, first );

			var can = sut.IsSatisfiedBy( 6776 );
			// Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.True( can );

			var second = sut.Get( 6776 );
			// Assert.Equal( 0, sut.CanCreateCalled );
			// Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( first, second );
		}

		static void BasicAutoValidationWith( IParameterizedSource<int, float> factory, ISpecification<int> specification, IExtendedFactory sut )
		{
			// Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );

			/*var invalid = specification.IsSatisfiedBy( "Message" );
			Assert.False( invalid );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );*/
			
			var cannot = specification.IsSatisfiedBy( 456 );
			Assert.False( cannot );
			// Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			factory.Get( 123 );

			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var can = specification.IsSatisfiedBy( 6776 );
			Assert.True( can );
			// Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 3, sut.CanCreateGenericCalled );

			// Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = factory.Get( 6776 );
			// Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 3, sut.CanCreateGenericCalled );
			// Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			sut.Reset();
		}

		

		interface IExtendedFactory : IParameterizedSource<int, float>, ISpecification<int>
		{
			// int CanCreateCalled { get; }

			// int CreateCalled { get; }

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
			// public int CanCreateCalled { get; private set; }

			// public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => /*CanCreateCalled =*/ /*CreateCalled =*/ CanCreateGenericCalled = CreateGenericCalled = 0;

			/*object IParameterizedSource<>.Get( object parameter )
			{
				CreateCalled++;
				return Get( (int)parameter );
			}*/

			[SupportsPolicies]
			public bool IsSatisfiedBy( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			[SupportsPolicies( AttributeInheritance = MulticastInheritance.Strict )]
			public virtual float Get( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}

			/*public bool IsSatisfiedBy( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && IsSatisfiedBy( (int)parameter );
			}*/
		}

		class ExtendedFactory : IExtendedFactory
		{
			// public int CanCreateCalled { get; private set; }

			// public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }
			public void Reset() => /*CanCreateCalled =*/ /*CreateCalled =*/ CanCreateGenericCalled = CreateGenericCalled = 0;

			/*public object Get( object parameter )
			{
				CreateCalled++;
				return Get( (int)parameter );
			}*/

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

			/*public bool IsSatisfiedBy( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && IsSatisfiedBy( (int)parameter );
			}*/
		}
	}
}