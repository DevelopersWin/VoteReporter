using DragonSpark.Commands;
using DragonSpark.ComponentModel;
using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Diagnostics.Exceptions
{
	// [ContentProperty( nameof(Policies) )]
	public class SetupExceptionHandling : CommandBase<object>
	{
		[Service, Required]
		public IServiceRepository Registry { [return: Required]get; set; }

		[Service, Required]
		public IExceptionHandler Handler { [return: Required]get; set; }

		public override void Execute( object parameter )
		{
			// Registry.Add( new ExceptionManager( Policies ) );

			/*Observable.FromEventPattern<>( )

			TaskScheduler.UnobservedTaskException += OnTaskSchedulerOnUnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException += OnXOnUnhandledException;*/
		}

		/*void OnXOnUnhandledException( object s, UnhandledExceptionEventArgs args ) => args.ExceptionObject.As<Exception>( Handler.Process );

		void OnTaskSchedulerOnUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs args ) => Handler.Process( args.Exception );*/

		// public DeclarativeCollection<Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.ExceptionPolicyDefinition> Policies { get; } = new DeclarativeCollection<Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.ExceptionPolicyDefinition>();
	}
}