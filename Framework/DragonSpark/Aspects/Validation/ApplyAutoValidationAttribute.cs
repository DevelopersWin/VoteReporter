using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	[IntroduceInterface( typeof(IAutoValidationController), OverrideAction = InterfaceOverrideAction.Ignore, AncestorOverrideAction = InterfaceOverrideAction.Ignore )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 )]
	[
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )
	]
	public class ApplyAutoValidationAttribute : ApplyAspectBase, IAutoValidationController
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = ToSourceDelegate();

		static Func<Type, IEnumerable<AspectInstance>> ToSourceDelegate()
		{
			try
			{
				return new AspectInstances( Defaults.Profiles.AsEnumerable() ).ToSourceDelegate();
			}
			catch ( Exception e )
			{
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {e}", null, null, null ));
				throw;
			}
			
		}

		readonly static Func<Type, bool> Specification = ToSpecificationDelegate();

		static Func<Type, bool> ToSpecificationDelegate()
		{
			try
{
							return new Specification( Defaults.Adapters ).ToSpecificationDelegate();
}
catch ( Exception e )
{
	MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {e}", null, null, null ));
	throw;
}

		}

		public ApplyAutoValidationAttribute() : this( Specification, DefaultSource ) {}
		protected ApplyAutoValidationAttribute( Func<Type, bool> specification, Func<Type, IEnumerable<AspectInstance>> source ) : base( specification, source ) {}

		IAutoValidationController Controller { get; set; }
		public sealed override void RuntimeInitializeInstance() => Controller = AutoValidationControllerFactory.Default.Get( Instance );

		bool ISpecification<object>.IsSatisfiedBy( object parameter ) => Controller.IsSatisfiedBy( parameter );
		void IAutoValidationController.MarkValid( object parameter, bool valid ) => Controller.MarkValid( parameter, valid );
		object IAutoValidationController.Execute( object parameter, IInvocation proceed ) => Controller.Execute( parameter, proceed );
	}
}