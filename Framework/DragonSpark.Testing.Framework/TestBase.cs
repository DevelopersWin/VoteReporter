using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Diagnostics;
using DragonSpark.Testing.Framework.Setup;
using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using System;
using System.Diagnostics;
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
			using ( var command = new AssignExecutionContextCommand().ExecuteWith( args.Method ) )
			{
				var output = args.Instance.AsTo<IValue<ITestOutputHelper>, Action<string>>( value => value.Item.WriteLine ) ?? ( s => { Debug.WriteLine( s ); } );
				using ( new TracingProfilerFactory( output, command.Provider.Get<ILoggerHistory>(), args.Method.Name ).Create() )
				{
					args.Proceed();
				}

				// Services.Get<IApplication>().With( application => application.Dispose() );
			}
		}
	}

	public class AssignExecutionContextCommand : AssignValueCommand<MethodBase>
	{
		readonly IWritableValue<IServiceProvider> serviceProvider;

		public AssignExecutionContextCommand() : this( CurrentServiceProvider.Instance, CurrentExecution.Instance ) {}

		public AssignExecutionContextCommand( IWritableValue<IServiceProvider> serviceProvider, IWritableValue<MethodBase> value ) : base( value )
		{
			this.serviceProvider = serviceProvider;
		}

		public IServiceProvider Provider => serviceProvider.Item;

		protected override void OnExecute( MethodBase parameter )
		{
			base.OnExecute( parameter );

			if ( serviceProvider.Item == null )
			{
				serviceProvider.Assign( DefaultServiceProvider.Instance.Item );
			}
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