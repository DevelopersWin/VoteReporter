using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public sealed class AutoValidationExtension : ExtensionBase
	{
		public static AutoValidationExtension Default { get; } = new AutoValidationExtension();
		AutoValidationExtension() : this( ExtensionPointProfiles.DefaultNested.Get, Defaults.ControllerSource ) {}

		readonly Func<Type, IEnumerable<ExtensionPointProfile>> source;
		readonly Func<object, IAutoValidationController> controllerSource;

		AutoValidationExtension( Func<Type, IEnumerable<ExtensionPointProfile>> source, Func<object, IAutoValidationController> controllerSource )
		{
			this.source = source;
			this.controllerSource = controllerSource;
		}

		public override void Execute( object parameter )
		{
			var active = new Active();
			var controller = controllerSource( parameter );
			
			foreach ( var pair in source( parameter.GetType() ) )
			{
				var context = pair.Validation.Get( parameter );
				context.Assign( new AutoValidationValidator( controller, context.Get(), active ) );
				// context.Add( validator );

				var invocationContext = pair.Execution.Get( parameter );
				var execution = new AutoValidationExecutor( controller, invocationContext.Get(), active );
				invocationContext.Assign( execution );
			}
		}
	}
}