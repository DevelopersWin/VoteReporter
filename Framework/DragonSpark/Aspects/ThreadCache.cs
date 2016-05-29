using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
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
		readonly AmbientStack<ThreadCache> stack;
		readonly ThreadCache item;

		public ThreadCacheContext() : this( () => new ThreadCache() ) {}

		public ThreadCacheContext( Func<ThreadCache> create ) : this( create, AmbientStack<ThreadCache>.Instance ) {}

		public ThreadCacheContext( Func<ThreadCache> create, AmbientStack<ThreadCache> stack  )
		{
			this.stack = stack;

			item = stack.GetCurrentItem() == null ? create() : null;

			if ( item != null )
			{
				stack.Value.Push( item );
			}
		}

		protected override void OnDispose( bool disposing )
		{
			if ( item != null )
			{
				item.Dispose();
				stack.Value.Pop();
			}
		}
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
	public class ThreadCacheAttribute : MethodInterceptionAspect
	{
		readonly AmbientStack<ThreadCache> current;
		public ThreadCacheAttribute() : this( AmbientStack<ThreadCache>.Instance ) {}

		protected ThreadCacheAttribute( AmbientStack<ThreadCache> current )
		{
			this.current = current;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var item = current.GetCurrentItem();
			if ( item != null )
			{
				var entry = new CacheEntry( args );
				args.ReturnValue = item.Get( entry );
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}
}