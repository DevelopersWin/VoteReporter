using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	public static class Keys
	{
		public static int For( MethodExecutionArgs args ) => KeyFactory.Instance.CreateUsing( args.Instance ?? args.Method.DeclaringType, args.Method, args.Arguments );

		// public static int For( MethodInterceptionArgs args ) => KeyFactory.Instance.CreateUsing( args.Instance ?? args.Method.DeclaringType, args.Method, args.Arguments );
	}

	[OnMethodBoundaryAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 3 ), ProvideAspectRole( StandardRoles.Validation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching )/*, AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )*/]
	public sealed class RecursionGuardAttribute : OnMethodBoundaryAspect
	{
		readonly int maxCallCount;
		readonly ICache<IDictionary<int, int>> cache;

		public RecursionGuardAttribute( int maxCallCount = 2 ) : this( new StoreCache<IDictionary<int, int>>( new ThreadLocalStoreCache<IDictionary<int, int>>( () => new Dictionary<int, int>() ) ), maxCallCount ) {}

		RecursionGuardAttribute( ICache<IDictionary<int, int>> cache, int maxCallCount = 2 )
		{
			this.maxCallCount = maxCallCount;
			this.cache = cache;
		}

		/*class Count : ThreadAmbientStore<int>
		{
			public Count( MethodExecutionArgs args ) : base( Keys.For( args ).ToString() ) {}

			int Update( bool up = true )
			{
				var amount = up ? 1 : -1;
				var result = Value + amount;
				Assign( result );
				return result;
			}

			public int Increment() => Update();

			public int Decrement() => Update( false );
		}*/

		public override void OnEntry( MethodExecutionArgs args )
		{
			var current = Current( args, 1 );
			if ( current >= maxCallCount )
			{
				throw new InvalidOperationException( $"Recursion detected in method {new MethodFormatter(args.Method).ToString( null, null )}" );
			}

			base.OnEntry( args );
		}

		int Current( MethodExecutionArgs args, int move )
		{
			var dictionary = cache.Get( args.Instance ?? args.Method.DeclaringType );
			var key = Keys.For( args );
			var result = dictionary[key] = dictionary.Ensure( key, i => 0 ) + move;
			return result;
		}

		public override void OnExit( MethodExecutionArgs args )
		{
			base.OnExit( args );
			Current( args, -1 );
		}
	}
}