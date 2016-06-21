using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects
{
	/*public sealed class CacheValueFactory
	{
		readonly ConcurrentDictionary<CacheEntry, object> items = new ConcurrentDictionary<CacheEntry, object>();

		public object Create( MethodInterceptionArgs parameter )
		{
			var code = KeyFactory.Create( parameter.Arguments.ToImmutableArray() );
			var entry = new CacheEntry( code, parameter );
			var result = items.GetOrAdd( entry, e => e.Get() );
			return result;
		}
	}*/

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect // , IInstanceScopedAspect
	{
		/*readonly Func<MethodInterceptionArgs, object> factory;

		public Freeze() : this( new CacheValueFactory().Create ) {}

		Freeze( Func<MethodInterceptionArgs, object> factory )
		{
			this.factory = factory;
		}*/

		readonly static ICache<Delegate, LookupDelegateInvoker> Lookup = new ActivatedCache<Delegate, LookupDelegateInvoker>();

		class LookupDelegateInvoker : CacheBase<object[], object>, IDelegateInvoker
		{
			readonly IDictionary<object[], object> items;

			public LookupDelegateInvoker() : this( new ConcurrentDictionary<object[], object>( StructuralEqualityComparer<object[]>.Instance ) ) {}

			LookupDelegateInvoker( IDictionary<object[], object> items )
			{
				this.items = items;
			}

			// public LookupDelegateInvoker Add( object[] key, Func<object> value )

			public object Invoke( object[] arguments ) => Get( arguments );

			public override bool Contains( object[] instance ) => items.ContainsKey( instance );

			public override bool Remove( object[] instance ) => items.Remove( instance );

			public override void Set( object[] instance, object value ) => items[instance] = value;

			public override object Get( object[] instance ) => items[ instance ];
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var parameter = new MethodInvocationParameter( args );
			args.ReturnValue = Factory.Instance.Create( parameter ).Invoke( parameter.Arguments );
		}

		class Factory : FactoryBase<MethodInvocationParameter, IDelegateInvoker>
		{
			public static Factory Instance { get; } = new Factory();

			public override IDelegateInvoker Create( MethodInvocationParameter parameter )
			{
				var result = Lookup.Get( Delegates.Default.Get( parameter.Instance ?? parameter.Method.DeclaringType ).Get( (MethodInfo)parameter.Method ) );
				var instance = parameter.Arguments;
				if ( !result.Contains( instance ) )
				{
					result.Set( instance, parameter.Proceed<object>() );
				}
				return result;
			}

			// static void Apply( LookupDelegateInvoker lookup, object[] instance, MethodInvocationParameter parameter ) => lookup.Set( instance, parameter.Proceed<object> );
		}

		/*object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs )
		{
			var result = new Freeze();
			//adviceArgs.Instance.Get( Properties.Services ).Register();
			return result;
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}*/
	}

	
	public class MethodInvocationSpecificationRepository : EntryRepositoryBase<ISpecification<MethodInvocationParameter>>
	{
		
	}

	public struct MethodInvocationParameter
	{
		readonly MethodInterceptionArgs args;
		// public static MethodInvocationParameter From( MethodInterceptionArgs args ) => new MethodInvocationParameter(  );

		public MethodInvocationParameter( MethodInterceptionArgs args ) : this( args.Method, args.Instance, args.Arguments.ToArray(), args ) {}

		public MethodInvocationParameter( MethodBase method, object instance, object[] arguments, MethodInterceptionArgs args )
		{
			Instance = instance;
			Method = method;
			Arguments = arguments;
			this.args = args;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
		public object[] Arguments { get; }
		public T Proceed<T>() => args.GetReturnValue<T>();
	}
}