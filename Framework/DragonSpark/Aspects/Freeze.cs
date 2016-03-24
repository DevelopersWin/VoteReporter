using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[PSerializable, ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		/*class Cached : ConnectedValue<object>
		{
			public Cached( object instance, string key, Func<object> create ) : base( instance, key, () => create() ) {}
		}*/

		// [Reference]
		Cache Items { get; set; }
		
		public override void RuntimeInitialize( MethodBase method ) => Initialize();

		void Initialize() => Items = new Cache();

		// [Synchronized]
		class Cache
		{
			// [Reference]
			readonly ConcurrentBag<int> keys = new ConcurrentBag<int>();

			// [Reference]
			readonly ConcurrentDictionary<int, object> items = new ConcurrentDictionary<int, object>();

			public object Get( MethodInterceptionArgs args )
			{
				var code = args.Arguments.Concat( new object[] { args.Method.DeclaringType, args.Method } ).Aggregate( 0x2D2816FE, ( current, item ) => current * 31 + ( item?.GetHashCode() ?? 0 ) );
				var b = !keys.Contains( code );
				var check = ( args.Method as MethodInfo )?.ReturnType != typeof(void) || b;
				if ( b )
				{
					keys.Add( code );
				}
				var result = check ? items.GetOrAdd( code, x => args.GetReturnValue() ) : args.ReturnValue;
				return result;
			}
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Items != null && ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) ) )
			{
				args.ReturnValue = Items.Get( args );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => MemberwiseClone();

		void IInstanceScopedAspect.RuntimeInitializeInstance() => Initialize();
	}
}