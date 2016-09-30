using DragonSpark.Aspects.Exceptions;
using DragonSpark.Commands;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Exceptions;
using DragonSpark.Sources;
using JetBrains.Annotations;
using Polly;
using System;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Aspects.Exceptions
{
	public class ApplyExceptionPolicyAttributeTests
	{
		[Fact]
		public void VerifyInstance()
		{
			var source = SuppliedRetryPolicySource<CustomException>.Default;
			Assert.Same( source.Get(), source.Get() );
		}

		[Fact]
		public void AppliedCommand()
		{
			var sut = new Command();
			var history = LoggingHistory.Default.Get();
			Assert.Empty( history.Events );
			Assert.Equal( 0, sut.Called );
			Assert.Throws<CustomException>( () => sut.Execute( true ) );
			Assert.Equal( 4, sut.Called );
			Assert.Equal( 3, history.Events.Count() );
		}

		/*[Fact]
		public void AppliedConstructor()
		{
			var history = LoggingHistory.Default.Get();
			Assert.Empty( history.Events );
			var sut = new ConstructedSubject();
			// Assert.Equal( 0, sut.Called );
			// Assert.Throws<CustomException>( () => sut.Execute( true ) );
			Assert.Equal( 3, sut.Called );
			Assert.Equal( 3, history.Events.Count() );
		}*/

		sealed class RetrySource<T> : SuppliedRetryPolicySource<T> where T : Exception
		{
			[UsedImplicitly]
			public new static ISource<Policy> Default { get; } = new Scope<Policy>( new RetrySource<T>().GlobalCache() );
			RetrySource() : base( new RetryPolicySource<T>( Time.None ), 3 ) {}
		}

		/*sealed class ConstructedSubject
		{
			[ApplyExceptionPolicyAspect( typeof(RetrySource<CustomException>) )]
			public ConstructedSubject()
			{
				if ( ++Called < 3 )
				{
					throw new CustomException();
				}
			}

			public int Called { get; set; }
		}*/

		[ApplyExceptionPolicy( typeof(RetrySource<CustomException>) )]
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