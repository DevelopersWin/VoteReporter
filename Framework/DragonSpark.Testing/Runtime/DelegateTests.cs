using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Testing.Framework;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using System;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Runtime
{
	public class DelegateTests : TestCollectionBase
	{
		public DelegateTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void DelegateType()
		{
			var cache = DragonSpark.Runtime.DelegateType.Default;
			Assert.Equal( typeof(Action), cache.Get( new Action( Command ).Method ) );
			Assert.Equal( typeof(Action<int>), cache.Get( new Action<int>( Command ).Method ) );
			Assert.Equal( typeof(Func<string, DateTime>), cache.Get( new Func<string, DateTime>( Factory ).Method ) );
		}

		void Command( int number ) {}

		void Command() {}

		DateTime Factory( string message ) => DateTime.Now;

		[Theory, AutoData]
		void Invoke( Mock<InvokeSubject> sut, string message )
		{
			var command = new Action<string>( sut.Object.Action );
			// var factory = new Func<string, DateTime>( Factory );
			var invoker = Invokers.Default.Get( sut.Object ).Get( command.Method );
			Assert.NotNull( invoker );
			Assert.IsType<ActionInvoker<string>>( invoker );
			Assert.Same( invoker, Invokers.Default.Get( sut.Object ).Get( command.Method ) );
			
			sut.Verify( subject => subject.Action( message ), Times.Never() );

			invoker.Invoke( message.ToItem() );

			sut.Verify( subject => subject.Action( message ), Times.Once() );

			// Assert.Equal( typeof(Action<int>), type );
		}

		public class InvokeSubject
		{
			public void Action() { }
			public virtual void Action( string message ) { }
		}

		/*[Fact]
		public void BasicInvocation()
		{
			Action instance = null;
			var called = false;
			instance = () =>
					   {
						   var current = Invocation.GetCurrent();
						   Assert.Exists( current );
						   Assert.Same( instance, current );
						   called = true;
					   };
			var action = Invocation.Create( instance );
			Assert.Null( Invocation.GetCurrent() );
			Assert.False( called );
			action();
			Assert.True( called );
			Assert.Null( Invocation.GetCurrent() );
		}

		[Theory, AutoData]
		void Closure( ClosureContext sut )
		{
			var source = new Action( sut.Original );
			var action = RelayDelegateProperty<Action>.Default.GetDirect( source.Method );

			Assert.Equal( 0, sut.OriginalCalled );
			Assert.Equal( 0, sut.AdjustedCalled );
			action();

			Assert.Equal( 1, sut.OriginalCalled );
			Assert.Equal( 0, sut.AdjustedCalled );

			var closure = action.Target as Closure;
			closure.Constants[0] = new Action( sut.Adjusted );


			action();
			Assert.Equal( 1, sut.OriginalCalled );
			Assert.Equal( 1, sut.AdjustedCalled );
		}

		class ClosureContext
		{
			public void Original() => OriginalCalled++;
			public int OriginalCalled { get; private set; }

			public void Adjusted() => AdjustedCalled++;
			public int AdjustedCalled { get; private set; }
		}

		[Theory, AutoData]
		void Empty()
		{
			var method = typeof(EmptyClass).GetMethod( nameof(EmptyClass.Method) );
			var empty = EmptyDelegateProperty<ActionWithParameter>.Default.GetDirect( method );
			Assert.IsType<ActionWithParameter>( empty );
		}

		class EmptyClass
		{
			public void Method( string first, bool second, DateTime third ) {}
		}

		public delegate void ActionWithParameter( string first, bool second, DateTime third );

		[Theory, AutoData]
		void CreateAndAdjust( Adjusted sut )
		{
			var source = new Action( sut.Call );
			var empty = EmptyDelegateProperty<Action>.Default.GetDirect( source.Method );
			empty();
			Assert.Equal( 0, sut.CallCalled );
			var relay = RelayDelegateProperty<Action>.Default.GetDirect( source.Method );
			var closure = relay.Target as Closure;
			relay();
			Assert.Equal( 0, sut.CallCalled );
			closure.Constants[1] = source;
			relay();
			Assert.Equal( 1, sut.CallCalled );
			Assert.Equal( 0, sut.FactoryCalled );
		}

		[Theory, AutoData]
		void CreateAndAdjustFactory( Adjusted sut )
		{
			var source = new Func<DateTime, int>( sut.Factory );
			var empty = EmptyDelegateProperty<FactoryDate>.Default.GetDirect( source.Method );
			empty( DateTime.Now );
			Assert.Equal( 0, sut.FactoryCalled );
			var relay = RelayDelegateProperty<FactoryDate>.Default.GetDirect( source.Method );
			var closure = relay.Target as Closure;
			relay( DateTime.Now );
			Assert.Equal( 0, sut.FactoryCalled );
			closure.Constants[1] = new FactoryDate( source );
			relay( DateTime.Now );
			Assert.Equal( 1, sut.FactoryCalled );
			Assert.Equal( 0, sut.CallCalled );
		}

		delegate int FactoryDate( DateTime date );

		class Adjusted
		{
			public void Call() => CallCalled++;
			public int CallCalled { get; private set; }

			public int Factory( DateTime date ) => FactoryCalled++;
			public int FactoryCalled { get; private set; }
		}

		[Theory, AutoData]
		void FullWorkflow1( BasicClass sut, int number, string message )
		{
			var source = new Func<int, string>( sut.HelloWorld );
			CurrentDelegate.Set( source, message );
			var item = Property<Func<int, string>>.Instance.GetDirect( source );
			var result = item( number );
			Assert.Contains( number.ToString(), result );
			Assert.Contains( message, result );
		}

		[Theory, AutoData]
		void Performance( string message )
		{
			/*using ( new ProfilerFactory( Output.WriteLine ).Create( MethodBase.GetCurrentMethod() ) )
			{
				var item = new BasicClass();
				for ( int i = 0; i < 10000; i++ )
				{
					var source = new Action( item.Empty );
					Property<Action>.Instance.GetDirect( source );
				}
			}#1#

			/*using ( new ProfilerFactory( Output.WriteLine ).Create( MethodBase.GetCurrentMethod() ) )
			{
				for ( int i = 0; i < 10000; i++ )
				{
					var source = new Action( new BasicClass().Empty );
					
				}
			}#1#
			
			/*var result = item( number );
			Assert.Contains( number.ToString(), result );
			Assert.Contains( message, result );#1#
		}

		class Property<T> : ContextAwareDelegateProperty<T> where T : class
		{
			public static Property<T> Instance { get; } = new Property<T>();

			Property() : base( new Factory( DelegateRelay.Instance ).ToDelegate() ) {}
		}

		class DelegateRelay : IDelegateRelay
		{
			public static DelegateRelay Instance { get; } = new DelegateRelay();

			public void Relay( DelegateWithParameterCache source, DelegateWithParameterCache destination )
			{
				var relay = source.Target as Closure;
				if ( relay != null && relay.Constants.Length == 2 )
				{
					relay.Constants[1] = destination;
				}
				else
				{
					throw new InvalidOperationException( "Provided delegate is not a Closure and does not have two constants." );
				}
			}
		}

		class BasicClass
		{
			public void Empty() {}

			public string HelloWorld( int number ) => $"Hello World: {number}. Message: {CurrentDelegate.Get<string>()}";
		}

		[Theory, AutoData]
		void PropertyContext( Factory factory, Parameter parameter, string name )
		{
			var @delegate = factory.Create( parameter );
			Assert.Null( factory.Current() );
			var result = @delegate( name );
			Assert.Null( factory.Current() );
			Assert.Equal( name, result.Name );
			Assert.Equal( parameter, result.Context );
		}

		class Factory : FactoryWithSpecificationBase<Parameter, Get>
		{
			readonly IAttachedProperty<Get, Parameter> property = new AttachedProperty<Get, Parameter>();

			public override Get Create( Parameter parameter ) => property.Apply( Get, parameter );

			Result Get( string name ) => new Result( name, property.Context() );

			public Parameter Current() => property.Context();
		}

		delegate Result Get( string name );

		class Parameter {}

		struct Result
		{
			public Result( string name, Parameter context )
			{
				Name = name;
				Context = context;
			}

			public string Name { get; }
			public Parameter Context { get; }
		}
		*/
	}
}