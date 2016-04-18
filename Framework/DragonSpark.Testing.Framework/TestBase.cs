using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;
using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using Serilog;
using Serilog.Core;
using System;
using System.Reflection;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework
{
	[Serializable, LinesOfCodeAvoided( 8 )]
	public class AssignExecutionContextAspect : MethodInterceptionAspect
	{
		public static AssignExecutionContextAspect Instance { get; } = new AssignExecutionContextAspect();

		AssignExecutionContextAspect() {}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			using ( new AssignExecutionContextCommand().ExecuteWith( args.Method ) )
			{
				var output = args.Instance.AsTo<IValue<ITestOutputHelper>, Action<string>>( value => value.Item.WriteLine ) ?? DebugOutputCommand.Instance.Run;
				using ( new Diagnostics.ProfilerFactory<Category.Debug>( output, Services.Get<ILogger>(), Services.Get<ILoggerHistory>() ).Create( args.Method ) )
				{
					args.Proceed();
				}
			}
		}
	}

	public static class MethodBaseExtensions
	{
		public static AssignExecutionContextCommand Assign( this MethodBase @this, ILoggerHistory history, LoggingLevelSwitch level ) => Assign( @this, new RecordingLoggerFactory( history, level ) );

		static AssignExecutionContextCommand Assign( this MethodBase @this, RecordingLoggerFactory factory )
		{
			var result = new AssignExecutionContextCommand().ExecuteWith( @this );
			DefaultServiceProvider.Instance.Assign( new ServiceProvider( factory ) );
			return result;
		}
	}

	public class AssignExecutionContextCommand : AssignValueCommand<MethodBase>
	{
		/*readonly IWritableValue<IServiceProvider> serviceProvider;
		readonly IWritableValue<ServiceProvider> defaultProvider;*/

		public AssignExecutionContextCommand() : this( /*CurrentServiceProvider.Instance,*/ CurrentExecution.Instance ) {}

		// public AssignExecutionContextCommand( Func<ServiceProvider> defaultProvider ) : this( new DefaultServiceProvider( defaultProvider ) ) {}

		// public AssignExecutionContextCommand( IWritableValue<ServiceProvider> defaultProvider ) : this( CurrentServiceProvider.Instance, defaultProvider, CurrentExecution.Instance ) {}

		public AssignExecutionContextCommand( /*IWritableValue<IServiceProvider> serviceProvider,*/ IWritableValue<MethodBase> value ) : base( value )
		{
			/*this.serviceProvider = serviceProvider;
			this.defaultProvider = defaultProvider;*/
		}

		protected override void OnExecute( MethodBase parameter )
		{
			base.OnExecute( parameter );

			/*if ( serviceProvider.Item == null )
			{
				serviceProvider.Assign( defaultProvider.Item );
			}*/
		}
	}

	[Disposable]
	public abstract class TestBase : FixedValue<ITestOutputHelper>
	{
		protected TestBase( ITestOutputHelper output )
		{
			Assign( output );
		}

		protected ITestOutputHelper Output => Item;

		protected virtual void Dispose( bool disposing ) {}
	}
}