using DragonSpark.Aspects.Extensibility;
using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Aspects.Extensions.Build;
using DragonSpark.Extensions;
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
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 )]
	[
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )
	]
	public class ApplyAutoValidationAttribute : ApplyAspectBase, IAutoValidationController
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = new AspectInstances( AutoValidation.DefaultProfiles.AsEnumerable() ).ToSourceDelegate();
		readonly static Func<Type, bool> Specification = Build.Specification.Default.ToSpecificationDelegate();

		public ApplyAutoValidationAttribute() : this( Specification, DefaultSource ) {}
		protected ApplyAutoValidationAttribute( Func<Type, bool> specification, Func<Type, IEnumerable<AspectInstance>> source ) : base( specification, source ) {}

		IAutoValidationController Controller { get; set; }
		public sealed override void RuntimeInitializeInstance() => Controller = AutoValidationControllerFactory.Default.Get( Instance );

		bool ISpecification<object>.IsSatisfiedBy( object parameter ) => Controller.IsSatisfiedBy( parameter );
		void IAutoValidationController.MarkValid( object parameter, bool valid ) => Controller.MarkValid( parameter, valid );
		object IAutoValidationController.Execute( object parameter, IInvocation proceed ) => Controller.Execute( parameter, proceed );
	}

	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	public abstract class ApplyAspectBase : InstanceLevelAspect, IAspectProvider
	{
		readonly Func<Type, bool> specification;
		readonly Func<Type, IEnumerable<AspectInstance>> source;

		protected ApplyAspectBase( Func<Type, bool> specification, Func<Type, IEnumerable<AspectInstance>> source )
		{
			this.specification = specification;
			this.source = source;
		}

		public override bool CompileTimeValidate( Type type ) => specification( type );

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => source( (Type)targetElement );
	}
}