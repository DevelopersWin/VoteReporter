using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class TaskExecutionContextTests : TestCollectionBase
	{
		public TaskExecutionContextTests( ITestOutputHelper output ) : base( output ) {}

		public static void Verify( MethodBase method )
		{
			var current = MethodContext.Instance.Get();
			if ( method != null && current != method )
			{
				throw new InvalidOperationException( $"Assigned Method is different from expected.  Expected: {method}.  Actual: {current}" );
			}
		}

		[Fact]
		public void Fact()
		{
			Assert.Equal( TaskContextStore.Instance.Value, TaskContext.Current() );
			Assert.Null( MethodContext.Instance.Get() );
		}

		[Theory, ExecutionContextAutoData]
		public void Theory()
		{
			var method = new Action( Theory ).Method;
			Verify( method );
			Assert.Equal( ExecutionContextStore.Instance.Value.Origin, TaskContext.Current() );
			Assert.Equal( method, MethodContext.Instance.Get() );
		}
/*
		[Fact, Wrapper]
		public void FactWrapped()
		{
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.Null( ExecutionContext.Instance.Value.Value );
		}

		[Theory, ExecutionContextAutoData, Wrapper]
		public void TheoryWrapped()
		{
			var method = GetType().GetMethod( nameof(TheoryWrapped) );
			Verify( method );
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.Exists( ExecutionContext.Instance.Value.Value );
			Assert.Equal( method, ExecutionContext.Instance.Value.Value );
		}*/
	}

	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	// ReSharper disable once TestClassNameDoesNotMatchFileNameWarning
	public class TaskedTaskExecutionContextTests : TestCollectionBase
	{
		public TaskedTaskExecutionContextTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public Task Fact()
		{
			var current = ExecutionContextStore.Instance.Value;
			Assert.Equal( ExecutionContextStore.Instance.Value.Origin, TaskContext.Current() );
			Assert.Null( MethodContext.Instance.Get() );
			return Task.Run( () =>
							 {
								 Assert.Same( current, ExecutionContextStore.Instance.Value );
								 Assert.NotEqual( ExecutionContextStore.Instance.Value.Origin, TaskContext.Current() );
								 Assert.Null( MethodContext.Instance.Get() );
							 } );
		}

		[Theory, ExecutionContextAutoData]
		public Task Theory()
		{
			var current = ExecutionContextStore.Instance.Value;
			Assert.Equal( TaskContextStore.Instance.Value, TaskContext.Current() );
			var method = new Func<Task>( Theory ).Method;
			Assert.Equal( method, MethodContext.Instance.Get() );
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContextStore.Instance.Value );
								TaskExecutionContextTests.Verify( method );
								Assert.NotEqual( ExecutionContextStore.Instance.Value.Origin, TaskContext.Current() );
								Assert.NotNull( MethodContext.Instance.Get() );
								Assert.Equal( method, MethodContext.Instance.Get() );
							 } );
		}
/*
		[Fact, Wrapper]
		public Task FactWrapped()
		{
			var current = ExecutionContext.Instance.Value;
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.Null( ExecutionContext.Instance.Value.Value );
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Instance.Value );
								Assert.NotEqual( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
								Assert.Null( ExecutionContext.Instance.Value.Value );
							 } );
		}

		[Theory, ExecutionContextAutoData, Wrapper]
		public Task TheoryWrapped()
		{
			var current = ExecutionContext.Instance.Value;
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			var method = GetType().GetMethod( nameof(TheoryWrapped) );
			Assert.Equal( method, ExecutionContext.Instance.Value.Value );
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Instance.Value );
								TaskExecutionContextTests.Verify( method );
								Assert.NotEqual( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
								Assert.Exists( ExecutionContext.Instance.Value.Value );
								Assert.Equal( method, ExecutionContext.Instance.Value.Value );
							 } );
		}*/
	}

	class ExecutionContextAutoData : AutoDataAttribute
	{
		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var enumerable = base.GetData( methodUnderTest );
			TaskExecutionContextTests.Verify( methodUnderTest );
			return enumerable;
		}
	}

	/*class Wrapper : BeforeAfterTestAttribute
	{
		public override void Before( MethodInfo methodUnderTest ) => TaskExecutionContextTests.Verify();

		public override void After( MethodInfo methodUnderTest ) => TaskExecutionContextTests.Verify();
	}*/
}