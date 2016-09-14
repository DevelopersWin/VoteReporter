using DragonSpark.Aspects.Extensibility;
using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Aspects.Extensions.Build;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Extensions
{
	[IntroduceInterface( typeof(IAutoValidationController), OverrideAction = InterfaceOverrideAction.Ignore )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Class )]
	[
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )
	]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public class ApplyAutoValidationAttribute : InstanceLevelAspect, IAspectProvider, IAutoValidationController
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = AspectInstances.Default.ToSourceDelegate();
		readonly static Func<Type, bool> Specification = Build.Specification.Default.ToSpecificationDelegate();

		readonly Func<Type, bool> specification;
		readonly Func<Type, IEnumerable<AspectInstance>> source;

		public ApplyAutoValidationAttribute() : this( Specification, DefaultSource ) {}

		protected ApplyAutoValidationAttribute( Func<Type, bool> specification, Func<Type, IEnumerable<AspectInstance>> source )
		{
			this.specification = specification;
			this.source = source;
		}

		public override bool CompileTimeValidate( Type type ) => specification( type );

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => source( (Type)targetElement );

		IAutoValidationController Controller { get; set; }
		public sealed override void RuntimeInitializeInstance() => Controller = AutoValidationControllerFactory.Default.Get( Instance );

		bool ISpecification<object>.IsSatisfiedBy( object parameter ) => Controller.IsSatisfiedBy( parameter );
		void IAutoValidationController.MarkValid( object parameter, bool valid ) => Controller.MarkValid( parameter, valid );
		object IAutoValidationController.Execute( object parameter, IInvocation proceed ) => Controller.Execute( parameter, proceed );
	}
}