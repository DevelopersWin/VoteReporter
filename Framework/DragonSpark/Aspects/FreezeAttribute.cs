using DragonSpark.Runtime.Properties;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class FreezeAttribute : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly ArgumentCache cache = new ArgumentCache();

		public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.Get( args );

		public object CreateInstance( AdviceArgs adviceArgs ) => new FreezeAttribute();

		void PostSharp.Aspects.IInstanceScopedAspect.RuntimeInitializeInstance() {}

		class ArgumentCache : ArgumentCache<MethodInterceptionArgs>
		{
			public ArgumentCache() : base( args => args.Arguments.ToArray(), args => args.GetReturnValue() ) {}
		}
	}
}