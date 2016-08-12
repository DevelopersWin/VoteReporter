﻿using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Windows.Runtime;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Composition;
using System.Linq;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation ), ContainingTypeAndNested]
	public class ExceptionHandlerTests
	{
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void Handle( [Service, Frozen]ExceptionManager manager, ExceptionHandler sut, Exception plain, InvalidOperationException invalid, ArgumentException argument, ArgumentOutOfRangeException outOfRange, ArgumentNullException argumentNull )
		{
			var first = sut.Handle( plain );
			Assert.Same( plain, first.Exception );
			Assert.True( first.RethrowRecommended );

			var second = sut.Handle( invalid );
			Assert.NotSame( invalid, second.Exception );
			Assert.IsType<ApplicationException>( second.Exception );
			Assert.True( second.RethrowRecommended );
			Assert.Equal( ExceptionManagerFactory.ExceptionWrapped, second.Exception.Message );
			Assert.Same( invalid, second.Exception.InnerException );

			var third = sut.Handle( argument );
			Assert.NotSame( argument, third.Exception );
			Assert.IsType<ApplicationException>( third.Exception );
			Assert.True( third.RethrowRecommended );
			Assert.Equal( ExceptionManagerFactory.ExceptionReplaced, third.Exception.Message );
			Assert.Null( third.Exception.InnerException );

			var fourth = sut.Handle( outOfRange );
			Assert.Same( outOfRange, fourth.Exception );
			Assert.False( fourth.RethrowRecommended );

			var fifth = sut.Handle( argumentNull );
			Assert.Same( argumentNull, fifth.Exception );
			Assert.True( fifth.RethrowRecommended );
		}

		[Export]
		public class ExceptionManagerFactory : SourceBase<ExceptionManager>
		{
			internal const string ExceptionReplaced = "Exception Replaced", ExceptionWrapped = "Exception Wrapped";

			public override ExceptionManager Get()
			{
				var policies = new[]
				{
					new ExceptionPolicyDefinition( 
						ExceptionHandler.DefaultExceptionPolicy, 
						new[] { 
							new ExceptionPolicyEntry( typeof(InvalidOperationException), PostHandlingAction.ThrowNewException, 
								new WrapHandler( ExceptionWrapped, typeof(ApplicationException) ).ToItem() 
							),

							new ExceptionPolicyEntry( typeof(ArgumentException), PostHandlingAction.ThrowNewException, 
								new ReplaceHandler( ExceptionReplaced, typeof(ApplicationException) ).ToItem() 
							),
							new ExceptionPolicyEntry( typeof(ArgumentOutOfRangeException), PostHandlingAction.None, Enumerable.Empty<IExceptionHandler>() ),
							new ExceptionPolicyEntry( typeof(ArgumentNullException), PostHandlingAction.NotifyRethrow, Enumerable.Empty<IExceptionHandler>() )
						} 
					)
				};

				var result = new ExceptionManager( policies );
				return result;
			}
		}
	}
}