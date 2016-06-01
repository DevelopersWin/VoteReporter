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

		public static void Verify( MethodBase method )
		{
			/*var current = TaskContext.Current();
			if ( ExecutionContext.Instance.Value.Id != current )
			{
				throw new System.InvalidOperationException( $@"'{ExecutionContext.Instance.Value}' does not contain '{current}'" );
			}*/

			if ( method != null && ExecutionContext.Instance.Value.Value != method )
			{
				throw new System.InvalidOperationException( $"Assigned Method is different from expected.  Expected: {method}.  Actual: {ExecutionContext.Instance.Value.Value}" );
			}
		}

		[Fact]
		public void Fact()
		{
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.Null( ExecutionContext.Instance.Value.Value );
		}

		[Theory, ExecutionContextAutoData]
		public void Theory()
		{
			var method = GetType().GetMethod( nameof(Theory) );
			Verify( method );
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			Assert.NotNull( ExecutionContext.Instance.Value.Value );
			Assert.Equal( method, ExecutionContext.Instance.Value.Value );
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
			Assert.NotNull( ExecutionContext.Instance.Value.Value );
			Assert.Equal( method, ExecutionContext.Instance.Value.Value );
		}*/
	}

	// ReSharper disable once TestClassNameDoesNotMatchFileNameWarning
	public class TaskedTaskExecutionContextTests : TestCollectionBase
	{
		public TaskedTaskExecutionContextTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public Task Fact()
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

		[Theory, ExecutionContextAutoData]
		public Task Theory()
		{
			var current = ExecutionContext.Instance.Value;
			Assert.Equal( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
			var method = GetType().GetMethod( nameof(Theory) );
			Assert.Equal( method, ExecutionContext.Instance.Value.Value );
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Instance.Value );
								TaskExecutionContextTests.Verify( method );
								Assert.NotEqual( ExecutionContext.Instance.Value.Id, TaskContext.Current() );
								Assert.NotNull( ExecutionContext.Instance.Value.Value );
								Assert.Equal( method, ExecutionContext.Instance.Value.Value );
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
								Assert.NotNull( ExecutionContext.Instance.Value.Value );
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