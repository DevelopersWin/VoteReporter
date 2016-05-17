using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using System;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Runtime
{
	[Aspect]
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

	[Serializable]
	public class Aspect : InstanceLevelAspect
	{
		[OnMethodInvokeAdvice, MulticastPointcut( MemberName = "CanCreate" )]
		public void CanCreate( MethodInterceptionArgs args )
		{
			Instance.As<IFactoryWithParameter>( 
				factory =>
				{
					var workflow = new FactoryWorkflow( new AssignableFactory( factory, o => args.GetReturnValue<bool>() ) );
					var returnValue = workflow.IsValid( args.Arguments.Single() );
					args.ReturnValue = returnValue;
				} );
		}

		[OnMethodInvokeAdvice, MulticastPointcut( MemberName = "Create" )]
		public void Create( MethodInterceptionArgs args )
		{
			Instance.As<IFactoryWithParameter>( 
				factory =>
				{
					var workflow = new FactoryWorkflow( new AssignableFactory( factory, o => args.GetReturnValue() ) );
					var returnValue = workflow.Apply( args.Arguments.Single() );
					args.ReturnValue = returnValue;
				} );
		}

		class AssignableFactory : IFactoryWithParameter
		{
			readonly IFactoryWithParameter inner;
			readonly Func<object, bool> condition;
			readonly Func<object, object> factory;

			public AssignableFactory( IFactoryWithParameter inner, Func<object, object> factory ) : this( inner, inner.CanCreate, factory ) {}

			public AssignableFactory( IFactoryWithParameter inner, Func<object, bool> condition ) : this( inner, condition, inner.Create ) {}

			AssignableFactory( IFactoryWithParameter inner, Func<object, bool> condition, Func<object, object> factory )
			{
				this.inner = inner;
				this.condition = condition;
				this.factory = factory;
			}

			public bool CanCreate( object parameter ) => condition?.Invoke( parameter ) ?? inner.CanCreate( parameter );

			public object Create( object parameter ) => factory?.Invoke( parameter ) ?? inner.Create( parameter );
		}
	}

	public class ParameterWorkflowTests
	{
		[Fact]
		public void BasicTest()
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
			/*var instance = new MyClass();
			var number = instance.HelloWorld();
			Assert.NotEqual( 123, number );*/
		}

		/*class MyClass
		{
			[Aspect]
			public int HelloWorld()
			{
				return 123;
			}
		}*/
	}
}
