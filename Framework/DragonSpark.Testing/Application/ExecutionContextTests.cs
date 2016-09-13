using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Application;
using DragonSpark.Testing.Framework.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Application
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class ExecutionContextTests : TestCollectionBase
	{
		public ExecutionContextTests( ITestOutputHelper output ) : base( output ) {}

		public static void Verify( MethodBase method )
		{
			var current = MethodContext.Default.Get();
			if ( method != null && current != method )
			{
				throw new InvalidOperationException( $"Assigned Method is different from expected.  Expected: {method}.  Actual: {current}" );
			}
		}

		[Fact]
		public void Fact()
		{
			Assert.Equal( Identification.Default.Get(), Identifier.Current() );
			Assert.Null( MethodContext.Default.Get() );
		}

		[Theory, ExecutionContextAutoData]
		public void Theory()
		{
			var method = new Action( Theory ).Method;
			Verify( method );
			Assert.Equal( ExecutionContext.Default.Get().Origin, Identifier.Current() );
			Assert.Equal( method, MethodContext.Default.Get() );
		}
/*
		[Fact, Wrapper]
		public void FactWrapped()
		{
			Assert.Equal( TaskContext.Default.Value.Id, Identifier.Current() );
			Assert.Null( TaskContext.Default.Value.Value );
		}

		[Theory, ExecutionContextAutoData, Wrapper]
		public void TheoryWrapped()
		{
			var method = GetType().GetMethod( nameof(TheoryWrapped) );
			Verify( method );
			Assert.Equal( TaskContext.Default.Value.Id, Identifier.Current() );
			Assert.Exists( TaskContext.Default.Value.Value );
			Assert.Equal( method, TaskContext.Default.Value.Value );
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
			var current = ExecutionContext.Default.Get();
			Assert.Equal( ExecutionContext.Default.Get().Origin, Identifier.Current() );
			Assert.Null( MethodContext.Default.Get() );
			return Task.Run( () =>
							 {
								 Assert.Same( current, ExecutionContext.Default.Get() );
								 Assert.NotEqual( ExecutionContext.Default.Get().Origin, Identifier.Current() );
								 Assert.Null( MethodContext.Default.Get() );
							 } );
		}

		[Theory, ExecutionContextAutoData]
		public Task Theory()
		{
			var current = ExecutionContext.Default.Get();
			Assert.Equal( Identification.Default.Get(), Identifier.Current() );
			var method = new Func<Task>( Theory ).Method;
			Assert.Equal( method, MethodContext.Default.Get() );
			return Task.Run( () =>
							 {
								Assert.Same( current, ExecutionContext.Default.Get() );
								ExecutionContextTests.Verify( method );
								Assert.NotEqual( ExecutionContext.Default.Get().Origin, Identifier.Current() );
								Assert.NotNull( MethodContext.Default.Get() );
								Assert.Equal( method, MethodContext.Default.Get() );
							 } );
		}
/*
		[Fact, Wrapper]
		public Task FactWrapped()
		{
			var current = TaskContext.Default.Value;
			Assert.Equal( TaskContext.Default.Value.Id, Identifier.Current() );
			Assert.Null( TaskContext.Default.Value.Value );
			return Task.Run( () =>
							 {
								Assert.Same( current, TaskContext.Default.Value );
								Assert.NotEqual( TaskContext.Default.Value.Id, Identifier.Current() );
								Assert.Null( TaskContext.Default.Value.Value );
							 } );
		}

		[Theory, ExecutionContextAutoData, Wrapper]
		public Task TheoryWrapped()
		{
			var current = TaskContext.Default.Value;
			Assert.Equal( TaskContext.Default.Value.Id, Identifier.Current() );
			var method = GetType().GetMethod( nameof(TheoryWrapped) );
			Assert.Equal( method, TaskContext.Default.Value.Value );
			return Task.Run( () =>
							 {
								Assert.Same( current, TaskContext.Default.Value );
								TaskExecutionContextTests.Verify( method );
								Assert.NotEqual( TaskContext.Default.Value.Id, Identifier.Current() );
								Assert.Exists( TaskContext.Default.Value.Value );
								Assert.Equal( method, TaskContext.Default.Value.Value );
							 } );
		}*/
	}

	class ExecutionContextAutoData : AutoDataAttribute
	{
		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var enumerable = base.GetData( methodUnderTest );
			ExecutionContextTests.Verify( methodUnderTest );
			return enumerable;
		}
	}

	/*class Wrapper : BeforeAfterTestAttribute
	{
		public override void Before( MethodInfo methodUnderTest ) => TaskExecutionContextTests.Verify();

		public override void After( MethodInfo methodUnderTest ) => TaskExecutionContextTests.Verify();
	}*/
}