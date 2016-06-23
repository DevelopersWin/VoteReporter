namespace DragonSpark.Testing.Runtime
{
	/*public class EnabledStateTests
	{
		[Theory, AutoData]
		public void BasicValues( EnabledState sut )
		{
			sut.Enable( 123, true );
			Assert.True( sut.IsEnabled( 123 ) );

			sut.Enable( 456, true );
			Assert.True( sut.IsEnabled( 123 ) );
			Assert.True( sut.IsEnabled( 456 ) );

			var o = new object();
			sut.Enable( o, true );
			Assert.True( sut.IsEnabled( 123 ) );
			Assert.True( sut.IsEnabled( 456 ) );
			Assert.True( sut.IsEnabled( o ) );

			sut.Enable( 456, false );

			Assert.True( sut.IsEnabled( 123 ) );
			Assert.False( sut.IsEnabled( 456 ) );
			Assert.True( sut.IsEnabled( o ) );

			sut.Enable( o, false );

			Assert.True( sut.IsEnabled( 123 ) );
			Assert.False( sut.IsEnabled( 456 ) );
			Assert.False( sut.IsEnabled( o ) );

			sut.Enable( 123, false );

			Assert.False( sut.IsEnabled( 123 ) );
			Assert.False( sut.IsEnabled( 456 ) );
			Assert.False( sut.IsEnabled( o ) );
		}
	}*/
}