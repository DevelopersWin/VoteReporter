using DragonSpark.Diagnostics.Logging;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[ProvideAspectRole( StandardRoles.Tracing ), LinesOfCodeAvoided( 4 )]
	[
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )
	]
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public sealed class TimedAttribute : MethodInterceptionAspect
	{
		readonly Func<MethodBase, IDisposable> source;

		public TimedAttribute() : this( TimedOperationFactory.Default.ToSourceDelegate() ) {}

		public TimedAttribute( string template ) : this( new TimedOperationFactory( template ).ToSourceDelegate() ) {}

		TimedAttribute( Func<MethodBase, IDisposable> source )
		{
			this.source = source;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			using ( source( args.Method ) )
			{
				base.OnInvoke( args );
			}
		}
	}
}