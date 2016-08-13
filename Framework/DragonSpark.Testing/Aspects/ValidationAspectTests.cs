using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Testing.Aspects
{
	public class ValidationAspectTests
	{
		/*[Fact]
		public void Validation()
		{
			var sut = new TestFactory();
			var factory = sut.WithAutoValidation().To<IValidatedParameterizedSource>();
			factory.IsValid( new object() );

			Assert.False( sut.Called );
			Assert.True( sut.GenericCalled );
		}*/

		public class TestFactory : IValidatedParameterizedSource<object, object>
		{
			public bool Called { get; set; }
			public bool GenericCalled { get; set; }

			bool IValidatedParameterizedSource.IsValid( object parameter )
			{
				Called = true;
				return IsValid( parameter );
			}

			public virtual bool IsValid( object parameter )
			{
				GenericCalled = true;
				return parameter.IsAssigned();
			}

			object IParameterizedSource.Get( object parameter ) => Get( parameter );

			public object Get( object parameter ) => null;
			bool ISpecification<object>.IsSatisfiedBy( object parameter ) => IsSatisfiedBy( parameter );

			public bool IsSatisfiedBy( object parameter ) => IsValid( parameter );
		}
		
		/*[Theory, AutoData]
		void CanExecuteAsExpected( ValidatedCommand sut )
		{
			var command = sut.WithAutoValidation();
			Assert.True( command.CanExecute( null ) );
			Assert.True( command.CanExecute( null ) );
		}

		[Theory, AutoData]
		void ExecuteAsExpected( ValidatedCommand sut )
		{
			var command = sut.WithAutoValidation();
			Assert.True( command.CanExecute( null ) );
			Assert.False( sut.Executed );

			command.Execute( null );

			Assert.True( sut.Executed );

			sut.Reset();

			Assert.False( sut.Executed );

			command.Execute( null );

			Assert.False( sut.Executed );
		}

		[Theory, AutoData]
		void ExecuteAsBaseCommandAsExpected( ValidatedCommand command )
		{
			var sut = (ICommand)command.WithAutoValidation();

			Assert.True( sut.CanExecute( null ) );
			Assert.False( command.Executed );

			sut.Execute( null );

			Assert.True( command.Executed );

			command.Reset();

			Assert.False( command.Executed );

			sut.Execute( null );

			Assert.False( command.Executed );
		}*/

		class ValidatedCommand : CommandBase<object>
		{
			public ValidatedCommand() : base( new OnlyOnceSpecification() ) {}

			public bool Executed { get; private set; }

			public void Reset() => Executed = false;

			public override void Execute( object parameter ) => Executed = true;
		}
	}
}
