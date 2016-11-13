using DragonSpark.Activation;
using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources.Coercion;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using System;

namespace DragonSpark.Aspects.Validation
{
	[IntroduceInterface( typeof(IAutoValidationController) ), LinesOfCodeAvoided( 4 )]
	public sealed class ApplyAutoValidation : InstanceAspectBase, IAutoValidationController, IAspectProvider
	{
		readonly static Func<object, IAspect> Factory = AutoValidationControllerFactory.Default.To( ParameterConstructor<IAutoValidationController, ApplyAutoValidation>.Default ).Get;

		readonly IAutoValidationController controller;

		public ApplyAutoValidation() : base( Factory, Definition.Default ) {}

		[UsedImplicitly]
		public ApplyAutoValidation( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		bool IAutoValidationController.IsActive => controller.IsActive;
		bool IAutoValidationController.Handles( object parameter ) => controller.Handles( parameter );
		void IAutoValidationController.MarkValid( object parameter, bool valid ) => controller.MarkValid( parameter, valid );
		object IAutoValidationController.Execute( object parameter, IAdapter proceed ) => controller.Execute( parameter, proceed );
	}
}