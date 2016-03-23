using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Xunit;

namespace DragonSpark.Testing.Runtime
{
	public class CompositeCommandTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void Execute( Command command )
		{
			
			var sut = new CompositeCommand( command.ToItem() );
			Assert.False( command.Executed );
			sut.Execute( new object() );
			Assert.True( command.Executed );
		}

		class Command : Command<object>
		{
			public bool Executed { get; private set; }
			
			protected override void OnExecute( object parameter )
			{
				Executed = true;
			}
		}
	}
}