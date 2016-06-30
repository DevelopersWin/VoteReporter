using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	public interface IParameterHandlerAware : IParameterHandler
	{
		void Register( IParameterHandler handler );
	}

	public interface IConnectionOwner : IConnectionAware
	{
		void Connect( IConnectionWorker worker );
	}

	public interface IConnectionAware
	{
		void Initialize();
	}

	public abstract class ConnectionAwareBase : IConnectionAware
	{
		public void Initialize() => OnInitialize();

		protected virtual void OnInitialize() {}
	}

	public class CompositeParameterHandler : List<IParameterHandler>, IParameterHandler
	{
		public bool Handles( object parameter )
		{
			foreach ( var handler in this )
			{
				if ( handler.Handles( parameter ) )
				{
					return true;
				}
			}
			return false;
		}

		public object Handle( object parameter )
		{
			foreach ( var handler in this )
			{
				if ( handler.Handles( parameter ) )
				{
					return handler.Handle( parameter );
				}
			}
			return Placeholders.Null;
		}
	}

	public abstract class ConnectionOwnerBase : ConnectionAwareBase, IConnectionOwner
	{
		readonly List<IConnectionWorker> workers = new List<IConnectionWorker>();

		protected ConnectionOwnerBase()
		{
			Workers = workers;
		}

		public IEnumerable<IConnectionWorker> Workers { get; } // TODO: Address this.

		protected override void OnInitialize()
		{
			// workers.Sort( PriorityComparer.Instance );
			foreach ( var worker in workers )
			{
				worker.Initialize();
			}
			// workers.Clear();
		}

		public void Connect( IConnectionWorker worker ) => workers.Add( worker );
	}

	public interface IConnectionWorker : IConnectionAware, IPriorityAware {}

	public abstract class ConnectionAwareBase<T> : ConnectionAwareBase where T : IConnectionOwner
	{
		protected ConnectionAwareBase( T owner )
		{
			Owner = owner;
		}

		protected T Owner { get; }

		public virtual Priority Priority => Priority.Normal;
	}

	public abstract class ConnectionWorkerBase<T> : ConnectionAwareBase<T>, IConnectionWorker where T : IConnectionOwner
	{
		protected ConnectionWorkerBase( T owner ) : base( owner )
		{
			owner.Connect( this );
		}
	}

	/*public class MethodInvocationParameterPool : PoolableBuilderBase<MethodInvocationParameter>
	{
		public static MethodInvocationParameterPool Instance { get; } = new MethodInvocationParameterPool();

		protected override void Apply( MethodInvocationParameter parameter, object instance, MethodBase method, Arguments arguments, Func<object> proceed ) 
			=> parameter.Apply( instance, method, arguments.ToArray(), proceed );
	}

	public class MethodInvocationSingleParameterPool : PoolableBuilderBase<MethodInvocationSingleParameter>
	{
		public static MethodInvocationSingleParameterPool Instance { get; } = new MethodInvocationSingleParameterPool();

		protected override void Apply( MethodInvocationSingleParameter parameter, object instance, MethodBase method, Arguments arguments, Func<object> proceed ) 
			=> parameter.Apply( instance, method, arguments?[0], proceed );
	}

	public abstract class PoolableBuilderBase<T> where T : class, IMethodInvocationParameter, new()
	{
		readonly ObjectPool<T> pool;

		protected PoolableBuilderBase() : this( new PoolStore().Value ) {}

		protected PoolableBuilderBase( ObjectPool<T> pool )
		{
			this.pool = pool;
		}

		protected class PoolStore : FixedStore<ObjectPool<T>>
		{
			public PoolStore( int size = 128 )
			{
				Assign( new ObjectPool<T>( Create, size ) );
			}

			protected virtual T Create() => new T();
		}

		public PooledContext From( object instance, MethodBase method, Arguments arguments, Func<object> proceed )
		{
			var item = pool.Allocate();
			Apply( item, instance, method, arguments, proceed );
			var result = new PooledContext( this, item );
			return result;
		}

		protected abstract void Apply( T parameter, object instance, MethodBase method, Arguments arguments, Func<object> proceed );

		public virtual void Free( T item )
		{
			// item.Clear();
			pool.Free( item );
		}


		public struct PooledContext : IDisposable
		{
			readonly PoolableBuilderBase<T> owner;

			public PooledContext( PoolableBuilderBase<T> owner, T item )
			{
				this.owner = owner;
				Item = item;
			}

			public T Item { get; }

			public void Dispose() => owner.Free( Item );
		}
	}*/

	public struct MethodInvocationSingleArgumentParameter
	{
		public MethodInvocationSingleArgumentParameter( object instance, MethodBase method, object argument, Func<object> proceed )
		{
			Instance = instance;
			Method = method;
			Argument = argument;
			Proceed = proceed;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
		public object Argument { get; }
		public Func<object> Proceed { get; }
	}

	public struct MethodInvocationParameter
	{
		public MethodInvocationParameter( object instance, MethodBase method, object[] arguments, Func<object> proceed )
		{
			Instance = instance;
			Method = method;
			Arguments = arguments;
			Proceed = proceed;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
		public object[] Arguments { get; }
		public Func<object> Proceed { get; }
	}

	/*public class MethodInvocationSingleParameter : MethodInvocationParameterBase<object> {}

	public class MethodInvocationParameter : MethodInvocationParameterBase<object[]>
	{
		public MethodInvocationParameter() {}
	}*/

	/*public interface IMethodInvocationParameter
	{
		object Instance { get; }
		MethodBase Method { get; }
		object Argument { get; }
		Func<object> Proceed {get; }

		void Clear();
	}*/

	/*public abstract class MethodInvocationParameterBase<T> : IMethodInvocationParameter
	{
		public void Apply( object instance, MethodBase method, T argument, Func<object> proceed )
		{
			Instance = instance;
			Method = method;
			Argument = argument;
			Proceed = proceed;
		}
		

		public object Instance { get; private set; }
		public MethodBase Method { get; private set; }
		public T Argument { get; private set; }
		public Func<object> Proceed {get; private set; }

		public void Clear()
		{
			Instance = null;
			Method = null;
			Argument = default(T);
			Proceed = null;
		}

		object IMethodInvocationParameter.Argument => Argument;
	}*/

	public interface IConnectionWorkerHost : IWritableStore<IConnectionWorker> {}

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

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class ConnectionWorkerHostBase : MethodInterceptionAspect, IInstanceScopedAspect, IConnectionWorkerHost
	{
		readonly Func<object, IConnectionWorker> worker;
		
		protected ConnectionWorkerHostBase( Func<object, IConnectionWorker> worker )
		{
			this.worker = worker;
		}

		public object CreateInstance( AdviceArgs adviceArgs )
		{
			var result = (IConnectionWorkerHost)MemberwiseClone();
			var instance = worker( adviceArgs.Instance );
			result.Assign( instance );
			return result;
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
		public void Assign( IConnectionWorker item ) => Value = item;

		public IConnectionWorker Value { get; private set; }

		object IStore.Value => Value;

		void IWritableStore.Assign( object item ) => Value = (IConnectionWorker)item;
	}
}