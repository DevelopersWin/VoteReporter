using DragonSpark.Aspects.Implementations;
using DragonSpark.Commands;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Windows.Input;
using Xunit;

namespace DragonSpark.Testing.Aspects.Implementations
{
	public class EnsureGeneralizedImplementationsAttributeTests
	{
		[Fact]
		public void VerifyCommand()
		{
			var sut = new Command();
			Assert.False( sut is ISpecification<object> );
		}

		[Fact]
		public void VerifySpecification()
		{
			var sut = new Specification();
			Assert.IsAssignableFrom<ISpecification<object>>( sut );
		}

		[Fact]
		public void VerifySource()
		{
			var sut = new Source();
			Assert.IsAssignableFrom<IParameterizedSource<object, object>>( sut );
		}

		[EnsureGeneralizedImplementations]
		class Source : ParameterizedSourceBase<string, bool>
		{
			public override bool Get( string parameter ) => false;
		}

		[EnsureGeneralizedImplementations]
		class Specification : SpecificationBase<DateTime>
		{
			public override bool IsSatisfiedBy( DateTime parameter ) => false;
		}

		// [EnsureGeneralizedImplementations]
		class Command : ICommand<int>
		{
			bool ICommand.CanExecute( object parameter ) => false;

			void ICommand.Execute( object parameter ) {}

			public event EventHandler CanExecuteChanged = delegate {};
			bool ISpecification<int>.IsSatisfiedBy( int parameter ) => false;

			void ICommand<int>.Execute( int parameter ) {}
			void ICommand<int>.Update() {}
		}
	}
}