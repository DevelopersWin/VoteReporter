using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Runtime;
using System;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	public class PoliciesTests
	{
		[Fact]
		public void VerifyInstance() => Assert.Same( Defaults<CustomException>.Retry.Get(), Defaults<CustomException>.Retry.Get() );

		[Fact]
		public void AppliedCommand()
		{
			var policy = Retry.Create( Sources.Parameterized.Extensions.Fixed( new RetryPolicyParameterSource<CustomException>( i => TimeSpan.Zero ), 3 ) );
			var command = new Command();
			var applied = command.Apply( policy );
			var history = LoggingHistory.Default.Get();
			Assert.Empty( history.Events );
			Assert.Throws<CustomException>( () => applied.Execute( true ) );
			Assert.Equal( 4, command.Called );
			Assert.Equal( 3, history.Events.Count() );
		}

		sealed class Command : CommandBase<bool>
		{
			public int Called { get; private set; }

			public override void Execute( bool parameter )
			{
				Called++;
				if ( parameter )
				{
					throw new CustomException();
				}
			}
		}

		class CustomException : Exception {}
	}
}
