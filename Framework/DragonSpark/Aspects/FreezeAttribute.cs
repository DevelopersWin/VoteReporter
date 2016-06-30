using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using DragonSpark.Runtime;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public class FreezeAttribute : MethodInterceptionAspect, IInstanceScopedAspect
	{
		bool Single { get; set; }

		public override void RuntimeInitialize( MethodBase method ) => Single = method.GetParameters().Length == 1;

		public object CreateInstance( AdviceArgs adviceArgs ) => Single ? new SingleParameterFreeze() : (FreezeAttribute)new Freeze();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		sealed class SingleParameterFreeze : FreezeAttribute
		{
			readonly ConcurrentDictionary<object, object> cache = new ConcurrentDictionary<object, object>();

			public override void OnInvoke( MethodInterceptionArgs args ) => 
				args.ReturnValue = cache.GetOrAdd( args.Arguments[0], key => args.GetReturnValue() );
		}

		sealed class Freeze : FreezeAttribute
		{
			readonly ConcurrentDictionary<object[], object> cache = new ConcurrentDictionary<object[], object>( StructuralEqualityComparer<object[]>.Instance );

			public override void OnInvoke( MethodInterceptionArgs args ) => 
				args.ReturnValue = cache.GetOrAdd( args.Arguments.ToArray(), key => args.GetReturnValue() );
		}
	}
}