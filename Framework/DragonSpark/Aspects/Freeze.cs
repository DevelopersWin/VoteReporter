using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections;
using System.Collections.Concurrent;

namespace DragonSpark.Aspects
{
	public sealed class CacheValueFactory // : FactoryBase<MethodInterceptionArgs, object>
	{
		readonly Func<IList, int> factory;
		readonly ConcurrentDictionary<int, object> items = new ConcurrentDictionary<int, object>();

		public CacheValueFactory() : this( KeyFactory.Instance.Create ) {}

		CacheValueFactory( Func<IList, int> factory )
		{
			this.factory = factory;
		}

		public object Create( MethodInterceptionArgs parameter )
		{
			var code = factory( parameter.Arguments.ToArray() );
			var result = items.GetOrAdd( code, key => parameter.GetReturnValue() );
			if ( result == null )
			{
				// Debug.WriteLine( $"{items.GetHashCode()} - Code: {code}. Instance: {parameter.Instance}. Method: {parameter.Method}" );
			}
			return result;
		}
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly Func<MethodInterceptionArgs, object> factory;

		public Freeze() : this( new CacheValueFactory().Create ) {}

		Freeze( Func<MethodInterceptionArgs, object> factory )
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

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => new Freeze();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}
}