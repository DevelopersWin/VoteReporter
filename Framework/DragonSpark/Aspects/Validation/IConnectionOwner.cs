using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	static class Connections
	{
		public static ICache<IConnectionOwner> Owner { get; } = new Cache<IConnectionOwner>();
	}

	public interface IConnectionOwner : IConnectionAware
	{
		void Connect( IConnectionWorker worker );
	}

	public interface IConnectionAware
	{
		void Initialize();
	}

	abstract class ConnectionOwnerBase<T> : IConnectionOwner where T : class, IConnectionWorker
	{
		readonly IList<IConnectionWorker> workers = new List<IConnectionWorker>();
		readonly Lazy<ImmutableArray<T>> cached;

		protected ConnectionOwnerBase()
		{
			cached = new Lazy<ImmutableArray<T>>( Create );
		}

		ImmutableArray<T> Create() => workers.Purge().Prioritize().ToImmutableArray().CastArray<T>();

		public ImmutableArray<T> Workers => cached.Value;

		public void Initialize()
		{
			foreach ( var worker in Workers )
			{
				worker.Initialize();
			}
		}

		public void Connect( IConnectionWorker worker ) => workers.Add( worker );
	}

	class ConnectionOwner : ConnectionOwnerBase<ConnectionWorker>
	{
		readonly object instance;
		public ConnectionOwner( object instance )
		{
			this.instance = instance;
		}
	}

	public class ConnectionOwnerFactory : FactoryBase<object, IConnectionOwner>
	{
		public static Func<object, IConnectionOwner> Instance { get; } = new ConnectionOwnerFactory().Cached().ToDelegate();

		public override IConnectionOwner Create( object parameter ) => new ConnectionOwner( parameter );
	}

	public interface IConnectionWorker : IConnectionAware, IPriorityAware
	{
		object Work( MethodInvocationParameter parameter );
	}

	abstract class ConnectionWorkerBase<T> : IConnectionWorker where T : IConnectionOwner
	{
		protected ConnectionWorkerBase( T owner )
		{
			Owner = owner;
		}

		protected T Owner { get; }

		public abstract object Work( MethodInvocationParameter parameter );

		public virtual Priority Priority => Priority.Normal;

		public virtual void Initialize() {}
	}

	class ConnectionWorker : ConnectionWorkerBase<ConnectionOwner>
	{
		public ConnectionWorker( ConnectionOwner owner ) : base( owner ) {}

		public override object Work( MethodInvocationParameter parameter ) => $"Hello World: {parameter.Proceed<object>()}";
	}

	public struct MethodInvocationParameter
	{
		readonly MethodInterceptionArgs args;

		public MethodInvocationParameter( MethodInterceptionArgs args ) : this( args.Method, args.Instance, args.Arguments.ToArray(), args ) {}

		MethodInvocationParameter( MethodBase method, object instance, object[] arguments, MethodInterceptionArgs args )
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

	public interface IConnectionWorkerAware : IWritableStore<IConnectionWorker> {}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class ConnectionOwnerHostBase : InstanceLevelAspect
	{
		readonly Func<object, IConnectionOwner> factory;

		protected ConnectionOwnerHostBase( Func<object, IConnectionOwner> factory )
		{
			this.factory = factory;
		}

		public override void RuntimeInitializeInstance() => factory( Instance ).Initialize();
	}

	class ConnectionOwnerHost : ConnectionOwnerHostBase
	{
		public ConnectionOwnerHost() : base( ConnectionOwnerFactory.Instance ) {}
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class ConnectionWorkerHostBase : MethodInterceptionAspect, PostSharp.Aspects.IInstanceScopedAspect, IConnectionWorkerAware
	{
		readonly Func<object, IConnectionWorker> worker;
		
		protected ConnectionWorkerHostBase( Func<object, IConnectionWorker> worker )
		{
			this.worker = worker;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Value != null )
			{
				args.ReturnValue = Value.Work( new MethodInvocationParameter( args ) ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		public object CreateInstance( AdviceArgs adviceArgs )
		{
			var result = (IConnectionWorkerAware)MemberwiseClone();
			var instance = worker( adviceArgs.Instance );
			result.Assign( instance );
			return result;
		}

		void PostSharp.Aspects.IInstanceScopedAspect.RuntimeInitializeInstance() {}
		public void Assign( IConnectionWorker item ) => Value = item;

		public IConnectionWorker Value { get; private set; }

		object IStore.Value => Value;

		void IWritableStore.Assign( object item ) => Value = (IConnectionWorker)item;
	}

	public class ConnectionWorkerHost : ConnectionWorkerHostBase
	{
		public ConnectionWorkerHost() : base( ConnectionWorkerFactory.Instance ) {}
	}

	class ConnectionWorkerFactory : FactoryBase<object, IConnectionWorker>
	{
		public static Func<object, IConnectionWorker> Instance { get; } = new ConnectionWorkerFactory().Cached().ToDelegate();

		readonly Func<object, IConnectionOwner> factory;

		public ConnectionWorkerFactory() : this( ConnectionOwnerFactory.Instance ) {}

		public ConnectionWorkerFactory( Func<object, IConnectionOwner> factory )
		{
			this.factory = factory;
		}

		public override IConnectionWorker Create( object parameter )
		{
			var owner = factory( parameter );
			var result = new ConnectionWorker( (ConnectionOwner)owner );
			owner.Connect( result );
			return result;
		}
	}
}