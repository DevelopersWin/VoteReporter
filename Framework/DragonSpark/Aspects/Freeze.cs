using DragonSpark.Activation;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DragonSpark.Aspects
{
	// [ReaderWriterSynchronized]
	public class CacheValueFactory : FactoryBase<MethodInterceptionArgs, object>
	{
		public static CacheValueFactory Instance { get; } = new CacheValueFactory();

		// [Reference]
		readonly IDictionary<int, Lazy<object>> items = new Dictionary<int, Lazy<object>>();

		Lazy<object> Get( int code, MethodInterceptionArgs args )
		{
			lock ( items )
			{
				var add = !items.ContainsKey( code );
				if ( add )
				{
					items.Add( code, new Lazy<object>( args.GetReturnValue ) );
				}
				var check = add || ( args.Method as MethodInfo )?.ReturnType != typeof(void);
				var result = check ? items[code] : null;
				return result;
			}
		}

		// [Writer]
		protected override object CreateItem( MethodInterceptionArgs parameter )
		{
			var instance = parameter.Instance ?? parameter.Method.DeclaringType;
			var enumerable = new[] { instance, parameter.DeclarationIdentifier }.Concat( parameter.Arguments );
			var code = KeyFactory.Instance.Create( enumerable );
			var deferred = Get( code, parameter );
			var result = deferred != null ? deferred.Value : parameter.ReturnValue;
			return result;
			//return /*parameter.GetReturnValue()*/ null;
		}
	}

	[PSerializable, ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		bool Enabled { get; set; }

		/*public override void RuntimeInitialize( MethodBase method )
		{
			throw new InvalidOperationException( "WTF" );

			Message.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "0001", $"HELLO WTF", null, null, null ) );
		}*/
		void Initialize() => Enabled = true;

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Enabled && ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) ) )
			{
				args.ReturnValue = CacheValueFactory.Instance.Create( args );
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