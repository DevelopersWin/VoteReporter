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