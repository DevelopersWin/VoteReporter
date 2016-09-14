using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;

namespace DragonSpark.Aspects.Extensions
{
	/*public interface IExtensionAware
	{
		object Invoke( IExtensionPoint point, Arguments arguments, Func<object> proceed );
	}*/

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	[ProvideAspectRole( StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	public abstract class AutoValidationAspectBase : MethodInterceptionAspect {}

	/*public struct AspectInvocation : IInvocation
	{
		readonly Func<object> proceed;

		public AspectInvocation( IExtensionPoint point, Arguments arguments, Func<object> proceed )
		{
			Arguments = arguments;
			this.proceed = proceed;
		}

		public Arguments Arguments { get; }

		public object Invoke( object parameter )
		{
			Arguments.SetArgument( 0, parameter );
			var result = proceed();
			return result;
		}
	}*/
}