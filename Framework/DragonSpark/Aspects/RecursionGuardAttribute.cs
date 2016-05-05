using DragonSpark.Diagnostics;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;

namespace DragonSpark.Aspects
{
	/*[PSerializable, LinesOfCodeAvoided( 3 ), ProvideAspectRole( StandardRoles.Validation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public class AllowAttribute : MethodInterceptionAspect
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( args.Arguments.All( o => o != null ) )
			{
				base.OnInvoke( args );
			}
			else
			{
				// args.ReturnValue = args.Method.AsTo<MethodInfo, object>( info => info.ReturnType.Adapt().GetDefaultValue() );
			}
		}
	}*/

	public static class Keys
	{
		public static int For( MethodExecutionArgs args ) => KeyFactory.Instance.CreateUsing( args.Instance ?? args.Method.DeclaringType, args.Method, args.Arguments );

		public static int For( MethodInterceptionArgs args ) => KeyFactory.Instance.CreateUsing( args.Instance ?? args.Method.DeclaringType, args.Method, args.Arguments );
	}

	[PSerializable, LinesOfCodeAvoided( 3 ), ProvideAspectRole( StandardRoles.Validation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching )]
	public sealed class RecursionGuardAttribute : OnMethodBoundaryAspect
	{
		public RecursionGuardAttribute( int maxCallCount = 2 )
		{
			MaxCallCount = maxCallCount;
		}

		int MaxCallCount { get; set; }

		class Count : ThreadAmbientStore<int>
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
		}

		int Current { get; set; }

		public override void OnEntry( MethodExecutionArgs args )
		{
			if ( ++Current >= MaxCallCount )
			{
				throw new InvalidOperationException( $"Recursion detected in method {new MethodFormatter(args.Method).ToString( null, null )}" );
			}

			base.OnEntry( args );
		}

		public override void OnExit( MethodExecutionArgs args )
		{
			base.OnExit( args );
			// new Count( args ).Decrement();
			Current--;
		}
	}
}