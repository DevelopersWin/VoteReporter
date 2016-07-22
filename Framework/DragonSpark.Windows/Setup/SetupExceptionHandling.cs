using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.Setup.Commands;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using System.Threading.Tasks;
using System.Windows.Markup;
using PostSharp.Patterns.Contracts;
using IExceptionHandler = DragonSpark.Diagnostics.IExceptionHandler;

namespace DragonSpark.Windows.Setup
{
	[ContentProperty( nameof(Policies) )]
	public class SetupExceptionHandling : UnityCommand
	{
		[Locate, Required]
		public IServiceRegistry Registry { [return: Required]get; set; }

		public override void Execute( object parameter )
		{
			Registry.Register( new InstanceRegistrationParameter( new ExceptionManager( Policies ) ) );

			Container.TryResolve<IExceptionHandler>().With( ConfigureExceptionHandling );
		}
		
		protected virtual void ConfigureExceptionHandling( IExceptionHandler handler )
		{
			TaskScheduler.UnobservedTaskException += ( sender, args ) => handler.Process( args.Exception );
			AppDomain.CurrentDomain.With( x => x.UnhandledException += ( s, args ) =>
			{
				args.ExceptionObject.As<Exception>( handler.Process );
			} );
		}

		public DeclarativeCollection<Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.ExceptionPolicyDefinition> Policies { get; } = new DeclarativeCollection<Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.ExceptionPolicyDefinition>();
	}
}