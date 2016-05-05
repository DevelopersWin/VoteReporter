using System;
using DragonSpark.Activation;

namespace DragonSpark.Runtime.Values
{
	public class ExecutionContextStore<T> : DeferredStore<T>
	{
		public ExecutionContextStore( Func<T> create = null ) : base( () => new AssociatedStore<T>( Execution.Current, create ) ) {}

		// public ExecutionContextValue( string key, Func<T> create ) : base( () => new AssociatedValue<T>( Execution.Current, key, create ) ) {}
	}
}