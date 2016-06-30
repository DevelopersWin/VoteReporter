using DragonSpark.Activation;
using DragonSpark.Testing.Framework;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Runtime
{
	public class ParameterValidationCachingTests : TestCollectionBase
	{
		public ParameterValidationCachingTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void BasicCondition()
		{
			var sut = new Factory();
			var factory = sut.WithAutoValidation();
			var cannot = factory.CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );

			var can = factory.CanCreate( 123 );
			Assert.True( can );
			Assert.Equal( 2, sut.CanCreateCalled );

			Assert.Equal( 0, sut.CreateCalled );

			var created = factory.Create( 123 );
			Assert.Equal( 2, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 6776, created );
		}

		[Fact]
		public void ExtendedCheck()
		{
			var sut = new ExtendedFactory();
			var factory = sut.WithAutoValidation();
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );

			var invalid = factory.CanCreate( "Message" );
			Assert.False( invalid );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );

			var cannot = factory.CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );

			var can = factory.CanCreate( 6776 );
			Assert.True( can );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = factory.Create( 6776 );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
		}

		[Fact]
		public void ExtendedCheckPerformance()
		{
			var method = MethodBase.GetCurrentMethod();
			using ( method.Profile( Output.WriteLine ) ) {}

			using ( method.Profile( Output.WriteLine ) )
			{
				for ( int i = 0; i < 10000; i++ )
				{
					ExtendedCheck();
				}
			}
		}

		/*[Fact]
		public void AssignPerformance()
		{
			//var parameter = new MethodInvocationSingleParameter();
			var method = MethodBase.GetCurrentMethod();
			var proceed = new Func<object>( () => null );
			using ( method.Profile( Output.WriteLine ) ) {}

			/*using ( method.Profile( Output.WriteLine ) )
			{
				for ( int i = 0; i < 80000; i++ )
				{
					parameter.Apply( this, method, argument, proceed );
				}
			}

			using ( method.Profile( Output.WriteLine ) )
			{
				for ( int i = 0; i < 80000; i++ )
				{
					var p = new MethodInvocationSingleParameter();
					p.Apply( this, method, argument, proceed );
				}
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();#1#


			PooledPerformance( method, proceed );


			/*using ( method.Profile( Output.WriteLine ) )
			{
				for ( int i = 0; i < 80000; i++ )
				{
					Instance = this;
					Method = method;
					Argument = argument;
					Proceed = proceed;
				}
			}

			using ( method.Profile( Output.WriteLine ) )
			{
				for ( int i = 0; i < 80000; i++ )
				{
					InstanceField = this;
					MethodField = method;
					ArgumentField = argument;
					ProceedField = proceed;
				}
			}#1#
		}*/

		/*void PooledPerformance( MethodBase method, Func<object> proceed )
		{
			using ( method.Profile( Output.WriteLine ) )
			{
				for ( int i = 0; i < 80000; i++ )
				{
					// using ( MethodInvocationSingleParameterPool.Instance.From( this, method, null, proceed ) ) {}
				}
			}
		}*/

		
		/*[Fact]
		public void ExtendedCheckFullFeature()
		{
			for ( int i = 0; i < 10000; i++ )
			{
			var sut = new ExtendedFactory();
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );

			var factory = sut.To<IFactoryWithParameter>();
			var invalid = factory.CanCreate( "Message" );
			Assert.False( invalid );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );
			
			var cannot = factory.CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );

			var can = sut.CanCreate( 6776 );
			Assert.True( can );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = factory.Create( 6776 );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			}
		}*/

		/*[Fact]
		public void VerifyCommand()
		{
			var sut = new Command();
			Assert.Equal( 0, sut.CanExecuteCalled );
			var cannot = sut.CanExecute( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanExecuteCalled );
			
			var can = sut.CanExecute( 1212 );
			Assert.True( can );
			Assert.Equal( 2, sut.CanExecuteCalled );
			

			Assert.Equal( 0, sut.ExecuteCalled );
			
			sut.Execute( 1212 );
			Assert.Equal( 2, sut.CanExecuteCalled );
			Assert.Equal( 1, sut.ExecuteCalled );
			Assert.Equal( 1212, sut.LastResult.GetValueOrDefault() );
		}*/

		// [AutoValidation.Factory]
		public class Factory : IFactoryWithParameter
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return (int)parameter == 123;
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return 6776;
			}
		}

		// [AutoValidation.GenericFactory]
		// [ApplyAutoValidation]
		class ExtendedFactory : IFactory<int, float>
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && CanCreate( (int)parameter );
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return Create( (int)parameter );
			}

			public bool CanCreate( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public float Create( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}

		/*class Command : CommandBase<int>
		{
			public int CanExecuteCalled { get; private set; }

			public int ExecuteCalled { get; private set; }

			public int? LastResult { get; set; }

			public override bool CanExecute( int parameter )
			{
				CanExecuteCalled++;
				return parameter == 1212;
			}

			public override void Execute( int parameter )
			{
				ExecuteCalled++;
				LastResult = parameter;
			}
		}*/
	}
}
