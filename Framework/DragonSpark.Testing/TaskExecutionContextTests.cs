using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = DragonSpark.Testing.Framework.ExecutionContext;

namespace DragonSpark.Testing
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class TaskExecutionContextTests : TestCollectionBase
	{
		public TaskExecutionContextTests( ITestOutputHelper output ) : base( output ) {}

		public static void Verify( MethodBase method )
		{
			var current = ExecutionContext.Instance.Value;
			if ( method != null && current != method )
			{
				throw new InvalidOperationException( $"Assigned Method is different from expected.  Expected: {method}.  Actual: {current}" );
			}
		}

		[Fact]
		public void Fact()
		{
			Assert.Equal( ExecutionContextHost.Instance.Value, TaskContext.Current() );
			Assert.Null( ExecutionContext.Instance.Value );
		}

		[Theory, ExecutionContextAutoData]
		public void Theory()
		{
			var method = new Action( Theory ).Method;
			Verify( method );
			Assert.Equal( ExecutionContextHost.Instance.Value, TaskContext.Current() );
			Assert.NotNull( ExecutionContext.Instance.Value );
			Assert.Equal( method, ExecutionContext.Instance.Value );
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
			var current = ExecutionContext.Instance.Value;
			Assert.Equal( ExecutionContextHost.Instance.Value, TaskContext.Current() );
			Assert.Null( current );
			return Task.Run( () =>
							 {
								 Assert.Same( current, ExecutionContext.Instance.Value );
								 Assert.NotEqual( ExecutionContextHost.Instance.Value, TaskContext.Current() );
								 Assert.Null( ExecutionContext.Instance.Value );
							 } );
		}

		[Theory, ExecutionContextAutoData]
		public Task Theory()
		{
			var current = ExecutionContext.Instance.Value;
			Assert.Equal( ExecutionContextHost.Instance.Value, TaskContext.Current() );
			var method = new Func<Task>( Theory ).Method;
			Assert.Equal( method, ExecutionContext.Instance.Value );
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Instance.Value );
								TaskExecutionContextTests.Verify( method );
								Assert.NotEqual( ExecutionContextHost.Instance.Value, TaskContext.Current() );
								Assert.NotNull( ExecutionContext.Instance.Value );
								Assert.Equal( method, ExecutionContext.Instance.Value );
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