using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[PSerializable]
	[ProvideAspectRole( StandardRoles.Tracing ), LinesOfCodeAvoided( 3 )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class ProfileAttribute : OnMethodBoundaryAspect
	{
		public ProfileAttribute() {}

		public ProfileAttribute( [OfFactoryType] Type factoryType )
		{
			FactoryType = factoryType; 
		}

		Type FactoryType { get; set; }

		IProfiler Create( MethodBase method )
		{
			var type = FactoryType ?? Configure.Get<Diagnostics.Configuration>().Profiler.FactoryType;
			var result = Services.Get<IFactory<MethodBase, IProfiler>>( type ).Create( method );
			return result;
		}

		public override void OnEntry( MethodExecutionArgs args ) => args.MethodExecutionTag = Create( args.Method ).With( profiler => profiler.Start() );

		public override void OnYield( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Pause() );

		public override void OnResume( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Resume() );

		public override void OnExit( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Dispose() );
	}
}