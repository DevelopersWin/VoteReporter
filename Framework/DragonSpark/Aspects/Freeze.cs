using DragonSpark.Activation;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using DragonSpark.Configuration;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;

namespace DragonSpark.Aspects
{
	[Synchronized]
	public class CacheValueFactory : FactoryBase<MethodInterceptionArgs, object>
	{
		[Reference]
		readonly HashSet<int> codes = new HashSet<int>();

		[Reference]
		readonly IDictionary<int, object> items = new Dictionary<int, object>();

		object Get( int code, MethodInterceptionArgs args )
		{
			var check = Add( code, args ) || ( args.Method as MethodInfo )?.ReturnType != typeof(void);
			var result = check ? items[code] : null;
			return result;
		}

		bool Add( int code, MethodInterceptionArgs args )
		{
			var result = !codes.Contains( code );
			if ( result )
			{
				codes.Add( code );
				items.Add( code, args.GetReturnValue() );
			}
			return result;
		}

		protected override object CreateItem( MethodInterceptionArgs parameter )
		{
			var code = KeyFactory.Instance.Create( parameter.Arguments );
			var result = Get( code, parameter ) ?? parameter.ReturnValue;
			return result;
		}
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly CacheValueFactory factory;

		public Freeze() : this( new CacheValueFactory() ) {}

		public Freeze( CacheValueFactory factory )
		{
			this.factory = factory;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Configure.Load<EnableMethodCaching>().Value && ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) ) )
			{
				args.ReturnValue = factory.Create( args );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		// public override void RuntimeInitialize( MethodBase method ) => Initialize();

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => MemberwiseClone();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		// void Initialize() => Factory = new CacheValueFactory();
	}
}