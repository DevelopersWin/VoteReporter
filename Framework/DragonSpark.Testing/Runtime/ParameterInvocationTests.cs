using DragonSpark.Activation;
using DragonSpark.Aspects;
using Xunit;

namespace DragonSpark.Testing.Runtime
{
	public class ParameterInvocationTests // : TestCollectionBase
	{
		// public ParameterInvocationTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void BasicCondition()
		{
			var sut = new Factory();
			var cannot = sut.CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );

			var can = sut.CanCreate( 123 );
			Assert.True( can );
			Assert.Equal( 2, sut.CanCreateCalled );

			Assert.Equal( 0, sut.CreateCalled );

			var created = sut.Create( 123 );
			Assert.Equal( 2, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 6776, created );
		}

		/*[Fact]
		public void EqualityTest()
		{
			var one = 123;
			var two = 123;
			var hashOne = one.GetHashCode();
			var hashTwo = two.GetHashCode();
			Assert.Same( one, two );
		}*/

		/*class Mock
		{
			public void Hello() => Debug.WriteLine( "Hello World!" );
		}

		[Fact]
		public void GetAllTypesWith2()
		{
			var sut = new[] { GetType(), typeof(NormalPriority), typeof(ServiceLocator), typeof(AutoDataAttribute), typeof(FileSystemAssemblySource) }.Select( type => type.Assembly ).ToArray();

			var result = Parallel.For( 0, 100000, i =>
									{
										var items = sut.GetAllTypesWith<PriorityAttribute>();
			Assert.True( items.Select( tuple => tuple.Item2 ).Contains( typeof(NormalPriority) ) );

										/*var mock = new Mock();
										var methodInfo = typeof(Mock).GetMethod( nameof<>(Mock.Hello) );
										AssociatedContext.Property.Set( methodInfo, new DisposableAction( () => {} ) );
										new ApplicationOutputCommand().Run( new OutputCommand.Parameter( mock, methodInfo, mock.Hello ) );
										Framework.Setup.ExecutionContext.Instance.Verify(); // TODO: Remove.
										Framework.Setup.ExecutionContext.Instance.Value.Dispose();#1#
									} );
			Assert.True( result.IsCompleted );
		}

		[Fact]
		public void Stress()
		{
			var result = Parallel.For( 0, 10000, i =>
									{
										/*var mock = new Mock();
										var methodInfo = typeof(Mock).GetMethod( nameof<>(Mock.Hello) );
										AssociatedContext.Property.Set( methodInfo, new DisposableAction( () => {} ) );
										new ApplicationOutputCommand().Run( new OutputCommand.Parameter( mock, methodInfo, mock.Hello ) );
										Framework.Setup.ExecutionContext.Instance.Verify(); // TODO: Remove.
										Framework.Setup.ExecutionContext.Instance.Value.Dispose();#1#
									} );
			Assert.True( result.IsCompleted );
		}*/

		[Fact]
		public void ExtendedCheck()
		{
			// for ( int i = 0; i < 10000; i++ )
			{
				var sut = new ExtendedFactory();
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );
			var cannot = sut.CanCreate( (object)456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );

			var can = sut.CanCreate( 6776 );
			Assert.True( can );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = sut.Create( (object)6776 );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			}
		}

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

		[ValidatedFactory, ValidatedFactory.Supplemental]
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

			public bool ShouldValidate() => true;
		}

		[ValidatedGenericFactory]
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

			public bool ShouldValidate() => true;
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
