using DragonSpark.Aspects.Relay;
using System;
using Xunit;

namespace DragonSpark.Testing.Aspects.Relay
{
	public class ApplyRelayAttributeTests
	{
		[Fact]
		public void Verify()
		{
			var sut = new Command();
			Assert.False( sut.CanExecute( 123 ) );
			Assert.Equal( 0, sut.CanExecuteCalled );
			Assert.Equal( 1, sut.CanExecuteGenericCalled );
		}

		[ApplyRelay]
		class Command : DragonSpark.Commands.ICommand<int>
		{
			public event EventHandler CanExecuteChanged = delegate {};

			public int CanExecuteCalled { get; private set; }
			public int CanExecuteGenericCalled { get; private set; }

			public int ExecuteCalled { get; private set; }
			public int ExecuteGenericCalled { get; private set; }

			public int LastResult { get; private set; }

			public bool CanExecute( object parameter )
			{
				CanExecuteCalled++;
				return false;
			}

			public void Execute( object parameter )
			{
				ExecuteCalled++;
				// Execute( new Parameter( (int)parameter ) );
			}

			public bool IsSatisfiedBy( int parameter )
			{
				CanExecuteGenericCalled++;
				return parameter == 6776;
			}

			public void Execute( int parameter )
			{
				ExecuteGenericCalled++;
				LastResult = parameter;
			}

			void DragonSpark.Commands.ICommand<int>.Update() {}
		}
	}
}