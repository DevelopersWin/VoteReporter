using System;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DragonSpark.Testing
{
	public class TaskExecutionContextTests : TestCollectionBase
	{
		public TaskExecutionContextTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void Fact()
		{
			ExecutionContext.Instance.Verify();
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.Null( ExecutionContext.Instance.Value.Value );
		}

		[Theory, ExecutionContextAutoData]
		public void Theory()
		{
			ExecutionContext.Instance.Verify();
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.NotNull( ExecutionContext.Instance.Value.Value );
		}

		[Fact, Wrapper]
		public void FactWrapped()
		{
			ExecutionContext.Instance.Verify();
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.Null( ExecutionContext.Instance.Value.Value );
		}

		[Theory, ExecutionContextAutoData, Wrapper]
		public void TheoryWrapped()
		{
			ExecutionContext.Instance.Verify();
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.NotNull( ExecutionContext.Instance.Value.Value );
		}
	}

	// ReSharper disable once TestClassNameDoesNotMatchFileNameWarning
	public class TaskedTaskExecutionContextTests : TestCollectionBase
	{
		public TaskedTaskExecutionContextTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public Task Fact()
		{
			var current = ExecutionContext.Instance.Value;
			return Task.Run( () =>
							 {
								 Assert.Same( current, ExecutionContext.Instance.Value );
								 ExecutionContext.Instance.Verify();
								 Assert.NotEqual( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
								 Assert.Null( ExecutionContext.Instance.Value.Value );
							 } );
		}

		[Theory, ExecutionContextAutoData]
		public Task Theory()
		{
			var current = ExecutionContext.Instance.Value;
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Instance.Value );
								ExecutionContext.Instance.Verify();
								Assert.NotEqual( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
								Assert.NotNull( ExecutionContext.Instance.Value.Value );
							 } );
		}

		[Fact, Wrapper]
		public Task FactWrapped()
		{
			var current = ExecutionContext.Instance.Value;
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Instance.Value );
								ExecutionContext.Instance.Verify();
								Assert.NotEqual( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
								Assert.Null( ExecutionContext.Instance.Value.Value );
							 } );
		}

		[Theory, ExecutionContextAutoData, Wrapper]
		public Task TheoryWrapped()
		{
			var current = ExecutionContext.Instance.Value;
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Instance.Value );
								ExecutionContext.Instance.Verify();
								Assert.NotEqual( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
								Assert.NotNull( ExecutionContext.Instance.Value.Value );
							 } );
		}
	}

	class ExecutionContextAutoData : AutoDataAttribute
	{
		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			ExecutionContext.Instance.Verify();
			return base.GetData( methodUnderTest );
		}
	}

	class Wrapper : BeforeAfterTestAttribute
	{
		readonly bool expectedToVerify;

		public Wrapper( bool expectedToVerify = true )
		{
			this.expectedToVerify = expectedToVerify;
		}

		public override void Before( MethodInfo methodUnderTest ) => ExecutionContext.Instance.Verify();

		public override void After( MethodInfo methodUnderTest )
		{
			var action = new Action( ExecutionContext.Instance.Verify );
			if ( expectedToVerify )
			{
				action();
			}
			else
			{
				Assert.Throws<InvalidOperationException>( action );
			}
		}
	}
}