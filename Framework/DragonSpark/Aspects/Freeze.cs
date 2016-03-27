using DragonSpark.Activation;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Aspects
{
	// [ReaderWriterSynchronized]
	public class CacheValueFactory : FactoryBase<MethodInterceptionArgs, object>
	{
		// readonly object owner;
		public static CacheValueFactory Instance { get; } = new CacheValueFactory();

		// [Reference]
		readonly IDictionary<int, Lazy<object>> items = new Dictionary<int, Lazy<object>>();

		/*public CacheValueFactory( [Required] object owner )
		{
			this.owner = owner;
		}*/

		Lazy<object> Get( int code, MethodInterceptionArgs args )
		{
			lock ( items )
			{
				var add = !items.ContainsKey( code );
				if ( add )
				{
					var instance = args.Instance ?? args.Method.DeclaringType;
					/*var arguments = args.Arguments.Aggregate( "Arguments: ", ( s, o ) => $" Argument: {o} (Code: {o.GetHashCode()} - {KeyFactory.Instance.CreateUsing( o )})" );
					var message = $"Adding: {code} for {instance} ({instance.GetHashCode()}). {arguments}";
					Debug.WriteLine( "Output = {0}", message );*/
					items.Add( code, new Lazy<object>( args.GetReturnValue, LazyThreadSafetyMode.PublicationOnly ) );
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
			/*var arguments = parameter.Arguments.Aggregate( "Arguments: ", ( s, o ) => $" Argument: {o} (Code: {o.GetHashCode()} - {KeyFactory.Instance.CreateUsing( o )})" );
            var message = $"{this} is creating key: {code} for {instance} ({instance.GetHashCode()}). {arguments}";
            Debug.WriteLine( "Output = {0}", message );*/
			var result = deferred != null ? deferred.Value : parameter.ReturnValue;
			return result;
			//return /*parameter.GetReturnValue()*/ null;
		}
	}

	[PSerializable, ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		bool Enabled { get; set; }

		// public bool UsingInstance { get; set; }

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