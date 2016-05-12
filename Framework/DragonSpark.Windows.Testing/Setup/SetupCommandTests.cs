﻿using Xunit;

namespace DragonSpark.Windows.Testing.Setup
{
	public class SetupCommandTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Execute( Command sut )
		{
			sut.Execute( new object() );
			Assert.True( sut.Executed, "Didn't call" );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Update( Command sut )
		{
			var called = false;
			sut.CanExecuteChanged += ( sender, args ) => called = true;
			sut.Update();
			Assert.True( called, "Didn't call" );
		}

		/*[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void CallWithNonContext( Command<SetupCommandTests> sut )
		{
			var context = new object();
			Assert.Throws<InvalidOperationException>( () => sut.To<ICommand>().Execute( context ) );
		}*/
	}

	public class Command : Command<object> {}

	public class Command<T> : DragonSpark.Runtime.CommandBase<T>
	{
		public bool Executed { get; private set; }
		
		public override void Execute( T parameter ) => Executed = true;
	}
}