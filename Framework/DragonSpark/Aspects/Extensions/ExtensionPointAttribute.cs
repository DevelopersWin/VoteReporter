using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Extensions
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	[ProvideAspectRole( "Extensibility" )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class ExtensionPointAttribute : MethodInterceptionAspect
	{
		public override void RuntimeInitialize( MethodBase method ) => Point = ExtensionPoints.Default.Get( (MethodInfo)method );

		IExtensionPoint Point { get; set; }

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var context = Point.Get( args.Instance );
			if ( context.IsSatisfiedBy( args.Instance ) )
			{
				context.Assign( new AspectInvocation( args.Arguments, args.GetReturnValue ) );
				args.ReturnValue = context.Invoke( args.Arguments.GetArgument( 0 ) ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}
}