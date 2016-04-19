using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[PSerializable]
	public sealed class ProfileAttribute : OnMethodBoundaryAspect
	{
		public ProfileAttribute() {}

		public ProfileAttribute( [OfFactoryType] Type factoryType )
		{
			FactoryType = factoryType; 
		}

		Type FactoryType { get; set; }

		IProfiler Create( MethodBase method ) => Services.Get<IFactory<MethodBase, IProfiler>>( FactoryType ?? Diagnostics.Configuration.Default.ProfilerFactoryType ).Create( method );

		public override void OnEntry( MethodExecutionArgs args ) => args.MethodExecutionTag = Create( args.Method ).With( profiler => profiler.Start() );

		public override void OnYield( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Pause() );

		public override void OnResume( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Resume() );

		public override void OnExit( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( profiler => profiler.Dispose() );
	}
}