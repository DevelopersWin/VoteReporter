using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	public class ThreadCache : Disposable
	{
		readonly IDictionary<CacheEntry, object> cache;

		public ThreadCache() : this( new Dictionary<CacheEntry, object>() ) {}

		public ThreadCache( IDictionary<CacheEntry, object> cache )
		{
			this.cache = cache;
		}

		public object Get( CacheEntry entry ) => cache.Ensure( entry, item => item.Get() );

		protected override void OnDispose( bool disposing )
		{
			base.OnDispose( disposing );
			cache.Clear();
		}
	}

	class ThreadCacheContext : Disposable
	{
		readonly IStack<ThreadCache> stack;
		
		public ThreadCacheContext() : this( () => new ThreadCache().Configured( false ) ) {}

		public ThreadCacheContext( Func<ThreadCache> create ) : this( create, AmbientStack<ThreadCache>.Instance ) {}

		public ThreadCacheContext( Func<ThreadCache> create, AmbientStack<ThreadCache> stack  )
		{
			var item = stack.GetCurrentItem() == null ? create() : null;
			if ( item != null )
			{
				this.stack = stack.Value;
				this.stack.Push( item );
			}
		}

		protected override void OnDispose( bool disposing ) => stack?.Pop().Dispose();
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer), AspectPriority = -1 )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public class ThreadCacheEntryPoint : OnMethodBoundaryAspect
	{
		public override void OnEntry( MethodExecutionArgs args ) => args.MethodExecutionTag = new ThreadCacheContext();

		public override void OnExit( MethodExecutionArgs args )
		{
			var disposable = args.MethodExecutionTag as IDisposable;
			disposable?.Dispose();
		}
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer), AspectPriority = -1 )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public class ThreadCacheAttribute : MethodInterceptionAspect//, IInstanceScopedAspect
	{
		readonly AmbientStack<ThreadCache> current;
		public ThreadCacheAttribute() : this( AmbientStack<ThreadCache>.Instance ) {}

		protected ThreadCacheAttribute( AmbientStack<ThreadCache> current )
		{
			this.current = current;
		}
		
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var item = GetValue( args );
			if ( item != null )
			{
				args.ReturnValue = item;
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		object GetValue( MethodInterceptionArgs args )
		{
			if ( current != args.Instance )
			{
				var item = current.GetCurrentItem();
				if ( item != null )
				{
					var entry = new CacheEntry( args );
					var result = item.Get( entry );
					return result;
				}
			}
			return null;
		}

		/*object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => new ThreadCacheAttribute( current );

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}*/
	}
}