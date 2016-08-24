using DragonSpark.Sources;
using JetBrains.dotMemoryUnit;
using PostSharp.Patterns.Model;
using System;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework
{


	/*public class ApplicationOutputCommand : OutputCommand
	{
		public ApplicationOutputCommand() : base( method => new InitializeMethodCommand( AssociatedContext.Default.Get( method ).Dispose ) ) {}
	}*/

	/*public class InitializeMethodCommand : AssignCommand<MethodBase>
	{
		readonly Action complete;
		readonly Action<Assembly> initialize;

		public InitializeMethodCommand() : this( Delegates.Empty ) {}

		public InitializeMethodCommand( Action complete ) : this( AssemblyInitializer.Default.ToDelegate(), complete ) {}

		public InitializeMethodCommand( Action<Assembly> initialize, Action complete ) : this( ExecutionContext.Default.Value, initialize, complete ) {}

		InitializeMethodCommand( IWritableStore<MethodBase> store, Action<Assembly> initialize, Action complete ) : base( store )
		{
			this.initialize = initialize;
			this.complete = complete;
		}

		public override void Execute( MethodBase parameter )
		{
			initialize( parameter.DeclaringType.Assembly );
			base.Execute( parameter );
		}

		protected override void OnDispose() => complete();
	}*/

	[Disposable]
	public abstract class TestCollectionBase
	{
		protected TestCollectionBase( ITestOutputHelper output )
		{
			WriteLine = output.WriteLine;
			Output.Default.Assign( WriteLine );
			DotMemoryUnitTestOutput.SetOutputMethod( WriteLine );
		}

		protected Action<string> WriteLine { get; }

		protected virtual void Dispose( bool disposing ) {}
	}

	/*public interface ITestOutputAware
	{
		ITestOutputHelper Output { get; }
	}*/
}