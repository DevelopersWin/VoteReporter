using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Aspects
{
	public struct CacheEntry : IEquatable<CacheEntry>
	{
		readonly int code;
		readonly MethodInterceptionArgs factory;

		public CacheEntry( MethodInterceptionArgs args ) : this( KeyFactory.Create( args.Arguments.ToImmutableArray() ), args ) {}

		public CacheEntry( int code, MethodInterceptionArgs factory )
		{
			this.code = code;
			this.factory = factory;
		}

		public object Get() => factory.GetReturnValue();

		public bool Equals( CacheEntry other ) => code == other.code;

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is CacheEntry && Equals( (CacheEntry)obj ) );

		public override int GetHashCode() => code;

		public static bool operator ==( CacheEntry left, CacheEntry right ) => left.Equals( right );

		public static bool operator !=( CacheEntry left, CacheEntry right ) => !left.Equals( right );
	}

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
		
		public ThreadCacheContext() : this( () => new ThreadCache() ) {}

		public ThreadCacheContext( Func<ThreadCache> create ) : this( create, AmbientStack<ThreadCache>.Default ) {}

		public ThreadCacheContext( Func<ThreadCache> create, IStackSource<ThreadCache> stack  )
		{
			var item = stack.GetCurrentItem() == null ? create() : null;
			if ( item != null )
			{
				this.stack = stack.Get();
				this.stack.Push( item );
			}
		}

		protected override void OnDispose( bool disposing ) => stack?.Pop().Dispose();
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer), AspectPriority = -1 )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public sealed class ThreadCacheEntryPoint : OnMethodBoundaryAspect
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
		public ThreadCacheAttribute() : this( AmbientStack<ThreadCache>.Default ) {}

		protected ThreadCacheAttribute( AmbientStack<ThreadCache> current )
		{
			this.current = current;
		}
		
		public sealed override void OnInvoke( MethodInterceptionArgs args )
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