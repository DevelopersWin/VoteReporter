using DragonSpark.ComponentModel;
using DragonSpark.Diagnostics.Exceptions;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using PostSharp.Patterns.Contracts;
using System;
using System.Threading.Tasks;
using System.Windows.Markup;
using IExceptionHandler = DragonSpark.Diagnostics.Exceptions.IExceptionHandler;

namespace DragonSpark.Windows.Setup
{
	[ContentProperty( nameof(Policies) )]
	public class SetupExceptionHandling : CommandBase<object>
	{
		[Service, Required]
		public IServiceRepository Registry { [return: Required]get; set; }

		[Service, Required]
		public IExceptionHandler Handler { [return: Required]get; set; }

		public override void Execute( object parameter )
		{
			Registry.Add( new ExceptionManager( Policies ) );

			/*Observable.FromEventPattern<>( )

			TaskScheduler.UnobservedTaskException += OnTaskSchedulerOnUnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException += OnXOnUnhandledException;*/
		}

		void OnXOnUnhandledException( object s, UnhandledExceptionEventArgs args ) => args.ExceptionObject.As<Exception>( Handler.Process );

		void OnTaskSchedulerOnUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs args ) => Handler.Process( args.Exception );

		public DeclarativeCollection<Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.ExceptionPolicyDefinition> Policies { get; } = new DeclarativeCollection<Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.ExceptionPolicyDefinition>();
	}
}