using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects
{
	// [AutoValidation( false )]
	[Synchronized]
	public sealed class CacheValueFactory // : FactoryBase<MethodInterceptionArgs, object>
	{
		[Reference]
		readonly IDictionary<int, Lazy<object>> items = new Dictionary<int, Lazy<object>>();

		object Get( MethodInterceptionArgs args )
		{
			var code = KeyFactory.Instance.CreateUsing( args.Instance ?? args.Method.DeclaringType, args.Method, args.Arguments.ToArray() );
			var check = Add( code, args ) || ( args.Method as MethodInfo )?.ReturnType != typeof(void);
			var result = check ? items[code].Value : null;
			return result;
		}

		bool Add( int code, MethodInterceptionArgs args )
		{
			var result = !items.ContainsKey( code );
			if ( result )
			{
				// codes.Add( code );
				items.Add( code, new Lazy<object>( args.GetReturnValue ) );
			}
			return result;
		}

		// [Yielder]
		public object Create( MethodInterceptionArgs parameter ) => Get( parameter ) ?? parameter.ReturnValue;

		/*public void Flush()
		{
			codes.Clear();
			items.Clear();
		}*/
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect// , IInstanceScopedAspect
	{
		readonly Func<MethodInterceptionArgs, object> factory;

		public Freeze() : this( new CacheValueFactory().Create ) {}

		public Freeze( Func<MethodInterceptionArgs, object> factory )
		{
			this.factory = factory;
		}

		// public override bool CompileTimeValidate( MethodBase method ) => false;

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( /*Configure.Load<EnableMethodCaching>().Value &&*/ ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) ) )
			{
				args.ReturnValue = factory( args );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		/*object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => new Freeze();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}*/
	}
}