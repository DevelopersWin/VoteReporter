using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using Serilog.Core;
using System;
using System.Reflection;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework
{
	[Serializable, LinesOfCodeAvoided( 8 )]
	public sealed class ExecuteMethodAspect : MethodInterceptionAspect
	{
		public static ExecuteMethodAspect Instance { get; } = new ExecuteMethodAspect();

		ExecuteMethodAspect() {}

		public override void OnInvoke( MethodInterceptionArgs args ) => new ApplicationOutputCommand().Run( new OutputCommand.Parameter( args.Instance, args.Method, args.Proceed ) );
	}

	public class ApplicationOutputCommand : OutputCommand
	{
		public ApplicationOutputCommand() : base( method => new InitializeMethodCommand( method.Get( AssociatedContext.Property ).Dispose ) ) {}
	}

	public static class MethodBaseExtensions
	{
		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this, ILoggerHistory history, LoggingLevelSwitch level ) => AsCurrentContext( @this, new RecordingLoggerFactory( history, level ) );

		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this ) => AsCurrentContext( @this, new RecordingLoggerFactory() );

		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this, RecordingLoggerFactory factory )
		{
			var result = new InitializeMethodCommand().AsExecuted( @this );
			DefaultServiceProvider.Instance.Assign( new ServiceProvider( factory ) );
			return result;
		}
	}

	public class InitializeMethodCommand : DisposingCommand<MethodBase>
	{
		readonly Action complete;
		readonly Action<Assembly> initialize;

		public InitializeMethodCommand(  ) : this( Delegates.Empty ) {}

		public InitializeMethodCommand( Action complete ) : this( AssemblyInitializer.Instance.Run, complete ) {}

		public InitializeMethodCommand( Action<Assembly> initialize, Action complete )
		{
			this.initialize = initialize;
			this.complete = complete;
		}

		public override void Execute( MethodBase parameter ) => initialize( parameter.DeclaringType.Assembly );

		protected override void OnDispose()
		{
			base.OnDispose();
			complete();
		}
	}

	[Disposable]
	public abstract class TestCollectionBase : ITestOutputAware
	{
		protected TestCollectionBase( ITestOutputHelper output )
		{
			Output = output;
		}

		[Reference]
		public ITestOutputHelper Output { get; }

		protected virtual void Dispose( bool disposing ) {}
	}

	public interface ITestOutputAware
	{
		ITestOutputHelper Output { get; }
	}

	public static class Traits
	{
		public const string Category = "Category";

		public static class Categories
		{
			public const string Modularity = "Modularity", IoC = "IoC";
		}
	}
}