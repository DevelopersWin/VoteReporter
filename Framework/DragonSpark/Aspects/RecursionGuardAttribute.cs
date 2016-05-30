using DragonSpark.Diagnostics;
using DragonSpark.Runtime.Properties;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;

namespace DragonSpark.Aspects
{
	/*public static class Keys
	{
		public static int For( MethodExecutionArgs args ) => KeyFactory.Instance.CreateUsing( args.Instance ?? args.Method.DeclaringType, args.Method, args.Arguments );

		public static int For( MethodInterceptionArgs args ) => KeyFactory.Instance.CreateUsing( args.Instance ?? args.Method.DeclaringType, args.Method, args.Arguments );
	}*/

	[PSerializable, LinesOfCodeAvoided( 3 ), ProvideAspectRole( StandardRoles.Validation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching )/*, AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )*/]
	public sealed class RecursionGuardAttribute : OnMethodBoundaryAspect
	{
		readonly static IAttachedProperty<int> Property = new ThreadLocalAttachedProperty<int>();

		public RecursionGuardAttribute( int maxCallCount = 2 )
		{
			MaxCallCount = maxCallCount;
		}

		int MaxCallCount { get; set; }

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
			var current = Property.Get( args.Instance ) + 1;
			Property.Set( args.Instance, current );
			if ( current >= MaxCallCount )
			{
				throw new InvalidOperationException( $"Recursion detected in method {new MethodFormatter(args.Method).ToString( null, null )}" );
			}

			base.OnEntry( args );
		}

		public override void OnExit( MethodExecutionArgs args )
		{
			base.OnExit( args );
			var current = Property.Get( args.Instance ) - 1;
			Property.Set( args.Instance, current );
		}
	}
}