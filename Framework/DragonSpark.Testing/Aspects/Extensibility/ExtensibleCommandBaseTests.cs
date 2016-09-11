using DragonSpark.Aspects.Extensibility;
using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Commands;
using DragonSpark.Specifications;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace DragonSpark.Testing.Aspects.Extensibility
{
	public class ExtensibleCommandBaseTests
	{
		[Fact]
		public void Verify()
		{
			var sut = new Command();
			sut.Execute( 123 );
			Assert.Empty( sut.Parameters );

			const int valid = 6776;
			sut.Execute( valid );
			Assert.Equal( valid, Assert.Single( sut.Parameters ) );
		}

		[Fact]
		public void Specification()
		{
			var sut = new SpecificationCommand();
			Assert.False( sut.IsSatisfiedBy( 1234 ) );
			Assert.True( sut.IsSatisfiedBy( 6776 ) );
		}

		[Fact]
		public void AutoValidationOnlyOnce()
		{
			var sut = new PocoCommand().ExtendUsing( new OnlyOnceSpecification<int>() ).Extend( AutoValidationExtension.Default );

			Assert.Empty( sut.Parameters );
			sut.Execute( 123 );
			Assert.Equal( 123, Assert.Single( sut.Parameters ) );

			for ( int i = 0; i < 10; i++ )
			{
				sut.Execute( 123 );
			}
			Assert.Single( sut.Parameters );
		}

		class PocoCommand : ExtensibleCommandBase<int>
		{
			public override void Execute( int parameter ) => Parameters.Add( parameter );

			public ICollection<int> Parameters { get; } = new Collection<int>();
		}

		[ApplyAutoValidation]
		class Command : ExtensibleCommandBase<int>
		{
			public override bool IsSatisfiedBy( int parameter ) => parameter == 6776;

			public override void Execute( int parameter ) => Parameters.Add( parameter );

			public ICollection<int> Parameters { get; } = new Collection<int>();
		}

		// [ApplyExtensions]
		class SpecificationCommand : ExtensibleCommandBase<int>
		{
			public SpecificationCommand()
			{
				this.ExtendUsing( Specification.Default );
			}
			public override void Execute( int parameter ) {}

			// public ICollection<int> Parameters { get; } = new Collection<int>();

			class Specification : ISpecification<int>
			{
				public static Specification Default { get; } = new Specification();
				Specification() {}

				public bool IsSatisfiedBy( int parameter ) => parameter == 6776;
			}
		}
	}
}