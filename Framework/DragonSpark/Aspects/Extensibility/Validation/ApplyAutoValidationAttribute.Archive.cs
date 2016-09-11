using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict, PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : TypeLevelAspect, IAspectProvider
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = AspectInstances.Default.ToSourceDelegate();
		readonly static Func<Type, bool> Specification = AutoValidationTypeSpecification.Default.ToSpecificationDelegate();

		readonly Func<Type, bool> specification;
		readonly Func<Type, IEnumerable<AspectInstance>> source;

		public ApplyAutoValidationAttribute() : this( Specification, DefaultSource ) {}

		protected ApplyAutoValidationAttribute( Func<Type, bool> specification, Func<Type, IEnumerable<AspectInstance>> source )
		{
			this.specification = specification;
			this.source = source;
		}

		public override bool CompileTimeValidate( Type type ) => specification( type );

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			var result = type != null ? source( type ) : Items<AspectInstance>.Default;
			return result;
		}
	}
}
