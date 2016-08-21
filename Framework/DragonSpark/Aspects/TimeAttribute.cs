using DragonSpark.Diagnostics.Logging;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using SerilogTimings.Extensions;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class TimeAttributeBase : MethodInterceptionAspect
	{
		readonly string template;

		protected TimeAttributeBase() : this( "Executed Method '{@Method}'" ) {}

		protected TimeAttributeBase( string template )
		{
			this.template = template;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			using ( Logger.Instance.Get( args.Method ).TimeOperation( template, args.Method ) )
			{
				base.OnInvoke( args );
			}
		}
	}

	[ProvideAspectRole( StandardRoles.Tracing ), LinesOfCodeAvoided( 4 )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public sealed class TimeAttribute : TimeAttributeBase
	{
		public TimeAttribute() {}
		public TimeAttribute( string template ) : base( template ) {}
	}
}