using DragonSpark.Aspects.Validation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
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
		public static AspectHub Default { get; } = new AspectHub();
		AspectHub() {}
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public class FreezeAttribute : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly Func<object, IAspectHub> hubSource;
		readonly static Func<object, IAspectHub> HubSource = AspectHub.Default.ToDelegate();

		public FreezeAttribute() : this( HubSource ) {}

		protected FreezeAttribute( Func<object, IAspectHub> hubSource )
		{
			this.hubSource = hubSource;
		}

		public override void RuntimeInitialize( MethodBase method ) => Profile = new MethodProfile( (MethodInfo)method );

		MethodProfile Profile { get; set; }

		public object CreateInstance( AdviceArgs adviceArgs )
		{
			var result = Profile.Create( Profile.Method );

			hubSource( adviceArgs.Instance )?.Register( result );

			return result;
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		sealed class SingleParameterFreeze : InstanceFreezeBase
		{
			readonly IArgumentCache<object, object> cache;

			public SingleParameterFreeze( MethodInfo method ) : this( new ArgumentCache<object, object>(), method ) {}

			SingleParameterFreeze( IArgumentCache<object, object> cache, MethodInfo method ) : base( new CacheParameterHandler<object, object>( cache ), method  )
			{
				this.cache = cache;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.GetOrSet( args.Arguments[0], args.GetReturnValue );
		}

		struct MethodProfile
		{
			public MethodProfile( MethodInfo method ) : this( method, method.GetParameters().Length == 1 ? new Func<MethodInfo, FreezeAttribute>( info => new SingleParameterFreeze( info ) ) : ( info => new Freeze( info ) ) ) {}

			MethodProfile( MethodInfo method, Func<MethodInfo, FreezeAttribute> create )
			{
				Method = method;
				Create = create;
			}

			public MethodInfo Method { get; }
			public Func<MethodInfo, FreezeAttribute> Create { get; }
		}

		abstract class InstanceFreezeBase : FreezeAttribute, IParameterAwareHandler, IMethodAware
		{
			readonly IParameterAwareHandler handler;
			protected InstanceFreezeBase( IParameterAwareHandler handler, MethodInfo method )
			{
				this.handler = handler;
				Method = method;
			}

			public bool Handles( object parameter ) => handler.Handles( parameter );

			public bool Handle( object parameter, out object handled ) => handler.Handle( parameter, out handled );

			public MethodInfo Method { get; }
		}

		sealed class Freeze : InstanceFreezeBase
		{
			readonly IArgumentCache<object[], object> cache;

			public Freeze( MethodInfo method ) : this( new ArgumentCache<object[], object>(), method ) {}

			Freeze( IArgumentCache<object[], object> cache, MethodInfo method ) : base( new CacheParameterHandler<object[], object>( cache ), method  )
			{
				this.cache = cache;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.GetOrSet( args.Arguments.ToArray(), args.GetReturnValue );
		}

		/*class CacheParameterConstructor<TKey, T> : FactoryBase<InstanceMethod, T> where T : class
		{
			//readonly static Func<InstanceMethod, IArgumentCache<TKey, object>> DefaultCacheSource = RegisteredCacheFactory<TKey, object>.Default.Create;

			readonly Func<InstanceMethod, IArgumentCache<TKey, object>> cacheSource;
			readonly Func<IArgumentCache<TKey, object>, T> factory;

			public static CacheParameterConstructor<TKey, T> Default { get; } = new CacheParameterConstructor<TKey, T>();
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