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
	public class ProfileAttribute : OnMethodBoundaryAspect
	{
		public ProfileAttribute() : this( typeof(ProfilerFactory<Category.Debug>) ) {}

		public ProfileAttribute( [OfFactoryType] Type factoryType )
		{
			FactoryType = factoryType;
		}

		Type FactoryType { get; set; }

		IFactory<MethodBase, IProfiler> Factory { get; set; }

		public override void RuntimeInitialize( MethodBase method ) => Factory = Services.Get<IFactory<MethodBase, IProfiler>>( FactoryType );

		public override void OnEntry( MethodExecutionArgs args ) => args.MethodExecutionTag = Factory.Create( args.Method ).With( controller => controller.Start() );

		public override void OnYield( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( controller => controller.Pause() );

		public override void OnResume( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( controller => controller.Resume() );

		public override void OnExit( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IProfiler>( controller => controller.Dispose() );
	}
}