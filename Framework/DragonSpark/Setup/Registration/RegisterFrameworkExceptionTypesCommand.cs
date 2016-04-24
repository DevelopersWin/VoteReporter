using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Windows.Markup;

namespace DragonSpark.Setup.Registration
{
	[ContentProperty( nameof(Types) )]
	public class RegisterFrameworkExceptionTypesCommand : Command<object>
	{
		[Locate, Required]
		public ILogger MessageLogger { [return: Required]get; set; }

		protected override void OnExecute( object parameter )
		{
			MessageLogger.Information( Resources.RegisteringFrameworkExceptionTypes );
			Types.Each( ExceptionExtensions.RegisterFrameworkExceptionType );
		}

		public System.Collections.ObjectModel.Collection<Type> Types { get; } = new System.Collections.ObjectModel.Collection<Type>( new [] { typeof(ActivationException), typeof(ResolutionFailedException) } );
	}
}