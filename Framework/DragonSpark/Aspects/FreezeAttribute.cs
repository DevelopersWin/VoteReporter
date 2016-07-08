using DragonSpark.Activation;
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

		public object CreateInstance( AdviceArgs adviceArgs )
		{
			var instance = new InstanceMethod( adviceArgs.Instance, Profile.Method );
			var attribute = Profile.Create( instance );
			return attribute;
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		sealed class SingleParameterFreeze : FreezeAttribute
		{
			public static Func<InstanceMethod, FreezeAttribute> Factory { get; } = CacheParameterConstructor<object, SingleParameterFreeze>.Instance.Create;

			readonly IArgumentCache<object, object> cache;

			SingleParameterFreeze( IArgumentCache<object, object> cache )
			{
				this.cache = cache;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.GetOrSet( args.Arguments[0], args.GetReturnValue );
		}

		struct MethodProfile
		{
			public MethodProfile( MethodInfo method ) : this( method, method.GetParameters().Length == 1 ? SingleParameterFreeze.Factory : Freeze.Factory ) {}

			MethodProfile( MethodInfo method, Func<InstanceMethod, FreezeAttribute> create )
			{
				Method = method;
				Create = create;
			}

			public MethodInfo Method { get; }
			public Func<InstanceMethod, FreezeAttribute> Create { get; }
		}

		sealed class Freeze : FreezeAttribute
		{
			public static Func<InstanceMethod, FreezeAttribute> Factory { get; } = CacheParameterConstructor<object[], Freeze>.Instance.Create;

			readonly IArgumentCache<object[], object> cache;

			Freeze( IArgumentCache<object[], object> cache )
			{
				this.cache = cache;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.GetOrSet( args.Arguments.ToArray(), args.GetReturnValue );
		}

		class CacheParameterConstructor<TKey, T> : FactoryBase<InstanceMethod, T> where T : class
		{
			readonly static Func<InstanceMethod, IArgumentCache<TKey, object>> DefaultCacheSource = RegisteredCacheFactory<TKey, object>.Instance.Create;

			readonly Func<InstanceMethod, IArgumentCache<TKey, object>> cacheSource;
			readonly Func<IArgumentCache<TKey, object>, T> factory;

			public static CacheParameterConstructor<TKey, T> Instance { get; } = new CacheParameterConstructor<TKey, T>();
			CacheParameterConstructor() : this( DefaultCacheSource, ParameterConstructor<IArgumentCache<TKey, object>, T>.Default ) {}

			CacheParameterConstructor( Func<InstanceMethod, IArgumentCache<TKey, object>> cacheSource, Func<IArgumentCache<TKey, object>, T> factory )
			{
				this.cacheSource = cacheSource;
				this.factory = factory;
			}

			public override T Create( InstanceMethod parameter )
			{
				var cache = cacheSource( parameter );
				var result = factory( cache );
				return result;
			}
		}
	}
}