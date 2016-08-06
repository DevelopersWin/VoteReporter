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
			Assert.Equal( Identification.Instance.Value, Identifier.Current() );
			Assert.Null( MethodContext.Instance.Get() );
		}

		[Theory, ExecutionContextAutoData]
		public void Theory()
		{
			var method = new Action( Theory ).Method;
			Verify( method );
			Assert.Equal( ExecutionContext.Instance.Get().Origin, Identifier.Current() );
			Assert.Equal( method, MethodContext.Instance.Get() );
		}
/*
		[Fact, Wrapper]
		public void FactWrapped()
		{
			Assert.Equal( TaskContext.Instance.Value.Id, Identifier.Current() );
			Assert.Null( TaskContext.Instance.Value.Value );
		}

		[Theory, ExecutionContextAutoData, Wrapper]
		public void TheoryWrapped()
		{
			var method = GetType().GetMethod( nameof(TheoryWrapped) );
			Verify( method );
			Assert.Equal( TaskContext.Instance.Value.Id, Identifier.Current() );
			Assert.Exists( TaskContext.Instance.Value.Value );
			Assert.Equal( method, TaskContext.Instance.Value.Value );
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
			var current = ExecutionContext.Instance.Get();
			Assert.Equal( ExecutionContext.Instance.Get().Origin, Identifier.Current() );
			Assert.Null( MethodContext.Instance.Get() );
			return Task.Run( () =>
							 {
								 Assert.Same( current, ExecutionContext.Instance.Get() );
								 Assert.NotEqual( ExecutionContext.Instance.Get().Origin, Identifier.Current() );
								 Assert.Null( MethodContext.Instance.Get() );
							 } );
		}

		[Theory, ExecutionContextAutoData]
		public Task Theory()
		{
			var current = ExecutionContext.Instance.Get();
			Assert.Equal( Identification.Instance.Value, Identifier.Current() );
			var method = new Func<Task>( Theory ).Method;
			Assert.Equal( method, MethodContext.Instance.Get() );
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Instance.Get() );
								TaskExecutionContextTests.Verify( method );
								Assert.NotEqual( ExecutionContext.Instance.Get().Origin, Identifier.Current() );
								Assert.NotNull( MethodContext.Instance.Get() );
								Assert.Equal( method, MethodContext.Instance.Get() );
							 } );
		}
/*
		[Fact, Wrapper]
		public Task FactWrapped()
		{
			var current = TaskContext.Instance.Value;
			Assert.Equal( TaskContext.Instance.Value.Id, Identifier.Current() );
			Assert.Null( TaskContext.Instance.Value.Value );
			return Task.Run( () =>
							 {
								Assert.Same( current, TaskContext.Instance.Value );
								Assert.NotEqual( TaskContext.Instance.Value.Id, Identifier.Current() );
								Assert.Null( TaskContext.Instance.Value.Value );
							 } );
		}

		[Theory, ExecutionContextAutoData, Wrapper]
		public Task TheoryWrapped()
		{
			var current = TaskContext.Instance.Value;
			Assert.Equal( TaskContext.Instance.Value.Id, Identifier.Current() );
			var method = GetType().GetMethod( nameof(TheoryWrapped) );
			Assert.Equal( method, TaskContext.Instance.Value.Value );
			return Task.Run( () =>
							 {
								Assert.Same( current, TaskContext.Instance.Value );
								TaskExecutionContextTests.Verify( method );
								Assert.NotEqual( TaskContext.Instance.Value.Id, Identifier.Current() );
								Assert.Exists( TaskContext.Instance.Value.Value );
								Assert.Equal( method, TaskContext.Instance.Value.Value );
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