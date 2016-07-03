using DragonSpark.Activation;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public class FreezeAttribute : MethodInterceptionAspect, IInstanceScopedAspect
	{
		MethodProfile Profile { get; set; }

		public override void RuntimeInitialize( MethodBase method ) => Profile = new MethodProfile( (MethodInfo)method );

		public object CreateInstance( AdviceArgs adviceArgs ) => Profile.Create( Delegates.Default.Get( adviceArgs.Instance ).Get( Profile.Method ) );

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		sealed class SingleParameterFreeze : FreezeAttribute
		{
			readonly IArgumentCache<object, object> cache;

			SingleParameterFreeze( IArgumentCache<object, object> cache )
			{
				this.cache = cache;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.GetOrSet( args.Arguments[0], args.GetReturnValue );

			public class Factory : FromArgumentCacheFactoryBase<object, SingleParameterFreeze>
			{
				public static Func<Delegate, FreezeAttribute> Instance { get; } = new Factory().Create;
				Factory() : base( cache => new SingleParameterFreeze( cache ) ) {}
			}
		}

		struct MethodProfile
		{
			public MethodProfile( MethodInfo method ) : this( method, method.GetParameters().Length == 1 ? SingleParameterFreeze.Factory.Instance : Freeze.Factory.Instance ) {}

			MethodProfile( MethodInfo method, Func<Delegate, FreezeAttribute> create )
			{
				Method = method;
				Create = create;
			}

			public MethodInfo Method { get; }
			public Func<Delegate, FreezeAttribute> Create { get; }
		}

		sealed class Freeze : FreezeAttribute
		{
			readonly IArgumentCache<object[], object> cache;

			Freeze( IArgumentCache<object[], object> cache )
			{
				this.cache = cache;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.GetOrSet( args.Arguments.ToArray(), args.GetReturnValue );

			public sealed class Factory : FromArgumentCacheFactoryBase<object[], Freeze>
			{
				public static Func<Delegate, FreezeAttribute> Instance { get; } = new Factory().Create;
				Factory() : base( cache => new Freeze( cache ) ) {}
			}
		}
	}
}