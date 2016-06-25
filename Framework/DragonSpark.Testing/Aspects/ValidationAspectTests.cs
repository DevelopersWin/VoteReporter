using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using Ploeh.AutoFixture.Xunit2;
using System.Windows.Input;
using DragonSpark.Aspects;
using Xunit;

namespace DragonSpark.Testing.Aspects
{
	public class ValidationAspectTests
	{
		[Fact]
		public void Validation()
		{}

		
		[Theory, AutoData]
		void CanExecuteAsExpected( ValidatedCommand sut )
		{
			Assert.True( sut.CanExecute( null ) );
			Assert.False( sut.CanExecute( null ) );
		}

		[Theory, AutoData]
		void ExecuteAsExpected( ValidatedCommand sut )
		{
			Assert.True( sut.CanExecute( null ) );
			Assert.False( sut.Executed );

			sut.Execute( null );

			Assert.True( sut.Executed );

			sut.Reset();

			Assert.False( sut.Executed );

			sut.Execute( null );

			Assert.False( sut.Executed );
		}

		[Theory, AutoData]
		void ExecuteAsBaseCommandAsExpected( ValidatedCommand command )
		{
			var sut = (ICommand)command;

			Assert.True( command.CanExecute( null ) );
			Assert.False( command.Executed );

			sut.Execute( null );

			Assert.True( command.Executed );

			command.Reset();

			Assert.False( command.Executed );

			sut.Execute( null );

			Assert.False( command.Executed );
		}

		[AutoValidation.GenericCommand]
		class ValidatedCommand : CommandBase<object>
		{
			public ValidatedCommand() : base( new OnlyOnceSpecification() ) {}

			public bool Executed { get; private set; }

			public void Reset() => Executed = false;

			public override void Execute( object parameter ) => Executed = true;
		}
	}
}
