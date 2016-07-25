using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Aspects
{
	[OnMethodBoundaryAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Tracing ), LinesOfCodeAvoided( 3 )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public sealed class ProfileAttribute : OnMethodBoundaryAspect
	{
		readonly Type factoryType;

		public ProfileAttribute() {}

		public ProfileAttribute( [OfFactoryType, Optional] Type factoryType )
		{
			this.factoryType = factoryType;
		}

		/*IProfiler Create( MethodBase method )
		{
			var type = factoryType ?? ProfilerFactoryConfiguration.Instance.Get( method );
			var result = GlobalServiceProvider.GetService<IFactory<MethodBase, IProfiler>>( type ).Create( method );
			return result;
		}*/

		// public override void OnEntry( MethodExecutionArgs args ) => args.MethodExecutionTag = ProfilerFactoryConfiguration.Instance.Get( args.Method ).With( profiler => profiler.Start() );

		/*public override void OnYield( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Pause() );

		public override void OnResume( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Resume() );

		public override void OnExit( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Dispose() );*/
	}
}