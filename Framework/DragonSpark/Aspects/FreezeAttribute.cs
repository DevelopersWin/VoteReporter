using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Runtime.Properties;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public class AspectHub : Cache<IAspectHub>
	{
		public static AspectHub Instance { get; } = new AspectHub();
		AspectHub() {}
	}

	/*public abstract class GeneralFactory<T> : Cache<Func<object, T>> where T : class
	{
		protected GeneralFactory() : this( new Cache<Func<object, T>>() ) {}

		protected GeneralFactory( ICache<object, Func<object, T>> inner ) : base( inner.Get ) {}

		public T For( object instance ) => Get( instance )?.Invoke( instance );
	}*/

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public class FreezeAttribute : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly Func<object, IAspectHub> hubSource;
		readonly static Func<object, IAspectHub> HubSource = AspectHub.Instance.ToDelegate();

		public FreezeAttribute() : this( HubSource ) {}

		public FreezeAttribute( Func<object, IAspectHub> hubSource )
		{
			this.hubSource = hubSource;
		}

		public override void RuntimeInitialize( MethodBase method ) => Profile = new MethodProfile( method );

		MethodProfile Profile { get; set; }

		public object CreateInstance( AdviceArgs adviceArgs )
		{
			var result = Profile.Create( Profile.Method );

			hubSource( adviceArgs.Instance )?.Register( result );

			return result;
			

			// Profile.Create(  )

			/*var instance = new InstanceMethod( adviceArgs.Instance, Profile.Method );
			var attribute = Profile.Create( instance );
			return attribute;*/
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		sealed class SingleParameterFreeze : InstanceFreezeBase
		{
			readonly IArgumentCache<object, object> cache;

			public SingleParameterFreeze( MethodBase method ) : this( new ArgumentCache<object, object>(), method ) {}

			SingleParameterFreeze( IArgumentCache<object, object> cache, MethodBase method ) : base( new CacheParameterHandler<object, object>( cache ), method  )
			{
				this.cache = cache;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.GetOrSet( args.Arguments[0], args.GetReturnValue );
		}

		struct MethodProfile
		{
			public MethodProfile( MethodBase method ) : this( method, method.GetParameters().Length == 1 ? (Func<MethodBase, FreezeAttribute>)ParameterConstructor<MethodBase, SingleParameterFreeze>.Default : ParameterConstructor<MethodBase, Freeze>.Default ) {}

			MethodProfile( MethodBase method, Func<MethodBase, FreezeAttribute> create )
			{
				Method = method;
				Create = create;
			}

			public MethodBase Method { get; }
			public Func<MethodBase, FreezeAttribute> Create { get; }
		}

		abstract class InstanceFreezeBase : FreezeAttribute, IParameterAwareHandler, IMethodAware
		{
			readonly IParameterAwareHandler handler;
			protected InstanceFreezeBase( IParameterAwareHandler handler, MethodBase method )
			{
				this.handler = handler;
				Method = method;
			}

			public bool Handles( object parameter ) => handler.Handles( parameter );

			public bool Handle( object parameter, out object handled ) => handler.Handle( parameter, out handled );

			public MethodBase Method { get; }
		}

		sealed class Freeze : InstanceFreezeBase
		{
			readonly IArgumentCache<object[], object> cache;

			public Freeze( MethodBase method ) : this( new ArgumentCache<object[], object>(), method ) {}

			Freeze( IArgumentCache<object[], object> cache, MethodBase method ) : base( new CacheParameterHandler<object[], object>( cache ), method  )
			{
				this.cache = cache;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.GetOrSet( args.Arguments.ToArray(), args.GetReturnValue );
		}

		/*class CacheParameterConstructor<TKey, T> : FactoryBase<InstanceMethod, T> where T : class
		{
			//readonly static Func<InstanceMethod, IArgumentCache<TKey, object>> DefaultCacheSource = RegisteredCacheFactory<TKey, object>.Instance.Create;

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
		}*/
	}
}