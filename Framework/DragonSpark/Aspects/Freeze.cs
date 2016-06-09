using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public sealed class CacheValueFactory
	{
		readonly ConcurrentDictionary<CacheEntry, object> items = new ConcurrentDictionary<CacheEntry, object>();

		public object Create( MethodInterceptionArgs parameter )
		{
			var code = KeyFactory.Instance.Create( parameter.Arguments.ToImmutableArray() );
			var entry = new CacheEntry( code, parameter );
			var result = items.GetOrAdd( entry, e => e.Get() );
			return result;
		}
	}

	public struct CacheEntry : IEquatable<CacheEntry>
	{
		readonly int code;
		readonly MethodInterceptionArgs factory;

		public CacheEntry( MethodInterceptionArgs args ) : this( KeyFactory.Instance.Create( args.Arguments.ToImmutableArray() ), args ) {}

		public CacheEntry( int code, MethodInterceptionArgs factory )
		{
			this.code = code;
			this.factory = factory;
		}

		public object Get() => factory.GetReturnValue();

		public bool Equals( CacheEntry other ) => code == other.code;

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is CacheEntry && Equals( (CacheEntry)obj ) );

		public override int GetHashCode() => code;

		public static bool operator ==( CacheEntry left, CacheEntry right ) => left.Equals( right );

		public static bool operator !=( CacheEntry left, CacheEntry right ) => !left.Equals( right );
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

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( /*Configure.Load<EnableMethodCaching>().Value &&*/ !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) )
			{
				args.ReturnValue = factory( args );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs )
		{
			var result = new Freeze();
			//adviceArgs.Instance.Get( Properties.Services ).Register();
			return result;
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}

	public class MethodInvocationSpecificationRepository : EntryRepositoryBase<ISpecification<MethodInvocationParameter>>
	{
		
	}

	public struct MethodInvocationParameter
	{
		public static MethodInvocationParameter From( MethodInterceptionArgs args ) => new MethodInvocationParameter( args.Method, args.Instance, args.Arguments.ToArray(), args.GetReturnValue );

		public MethodInvocationParameter( MethodBase method, object instance, object[] arguments, Func<object> @continue )
		{
			Instance = instance;
			Method = method;
			Arguments = arguments;
			Continue = @continue;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
		public object[] Arguments { get; }
		public Func<object> Continue { get; }
	}
}