namespace DragonSpark.Configuration
{
	/*public interface IParameterizedConfiguration<T> : IParameterizedConfiguration<object, T> {}

	public interface IParameterizedConfiguration<TKey, TValue> : IParameterizedConfiguration<TKey, TValue> {}*/



	/*public interface IParameterizedConfiguration<T> : IParameterizedConfiguration<object, T> {}

	public interface IParameterizedConfiguration<TParameter, TResult> : IParameterizedSource<TParameter, TResult>, IAssignable<Func<TParameter, TResult>> {}*/

	/*public class ParameterizedConfiguration<T> : ParameterizedConfiguration<object, T>
	{
		public ParameterizedConfiguration( Func<object, T> factory ) : base( factory ) {}
		public ParameterizedConfiguration( IAssignableDelegatedParameterizedSource<object, T> source ) : base( source ) {}
		protected ParameterizedConfiguration( IAssignable<Func<object, T>> store, IParameterizedSource<object, T> cache ) : base( store, cache ) {}
	}*/



	/*public class ParameterizedConfiguration<TInstance, TValue> : Scope<ICache<TInstance, TValue>>,
/*CachedParameterizedScope<TInstance, TValue>, IAssignable<Func<TInstance, TValue>>#1#IParameterizedConfiguration<TInstance, TValue>
	{
		readonly IAssignableParameterizedSource<object, ICache<TInstance, TValue>> source;
		public ParameterizedConfiguration( Func<TInstance, TValue> factory ) : this( new Cache<ICache<TInstance, TValue>>( o => new Scope<Func<TInstance, TValue>>( factory.Self ) ) ) {}

		public ParameterizedConfiguration( IAssignableParameterizedSource<object, ICache<TInstance, TValue>> source ) : base( source )
		{
			this.source = source;
		}

		/*readonly IParameterizedSource<TInstance, TValue> source;
		readonly IAssignable<Func<TInstance, TValue>> assignable;

		public ParameterizedConfiguration( IParameterizedSource<TInstance, TValue> source, IAssignable<Func<TInstance, TValue>> assignable )
		{
			this.source = source;
			this.assignable = assignable;
		}#1#

		/*readonly IAssignable<Func<TInstance, TValue>> store;

		// public ParameterizedConfiguration( Func<TInstance, TValue> factory ) : this( new AssignableDelegatedParameterizedSource<TInstance, TValue>( factory ) ) {}

		public ParameterizedConfiguration( Func<TInstance, TValue> factory ) : this( source, source.ToCache() ) {}

		protected ParameterizedConfiguration( IAssignable<Func<TInstance, TValue>> store, IParameterizedSource<TInstance, TValue> cache ) : base( cache )
		{
			this.store = store;
		}

		public virtual void Assign( Func<TInstance, TValue> item ) => store.Assign( item );#1#
		public TValue Get( TInstance parameter ) => base.Get().Get( parameter );

		public void Assign( Func<TInstance, TValue> item ) => assignable.Assign( item );
	}*/

	/*public class FactoryExecutionScopeConfiguration<T> : ExecutionScopeConfiguration<T>
	{
		public FactoryExecutionScopeConfiguration( Func<T> factory ) : base( factory ) {}
		
		public override T Get( object parameter ) => Cache.Contains( parameter ) ? Cache.Get( parameter ) : defaultFactory();
	}*/

	/*public class ExecutionScopeConfiguration<T> : AssignableDelegatedParameterizedScope<T>
	{
		public ExecutionScopeConfiguration( Func<T> factory ) : this( new AssignableDelegatedParameterizedScope<T>( factory.Wrap() ) ) {}

		protected ExecutionScopeConfiguration( IAssignableDelegatedParameterizedSource<object, T> source ) : this( source, CacheFactory.Create<object, T>( source.Get ) ) {}

		protected ExecutionScopeConfiguration( IAssignable<Func<object, T>> store, ICache<object, T> cache ) : base( store, cache )
		{
			Cache = cache;
		}

		protected ICache<object, T> Cache { get; }

		public override void Assign( Func<object, T> item )
		{
			Cache.Remove( Execution.Current() );
			base.Assign( item );
		}
	}*/

	/*public class AssignableDelegatedParameterizedScope<T> : AssignableDelegatedParameterizedScope<object, T>
	{
		public AssignableDelegatedParameterizedScope( Func<T> defaultFactory ) : base( defaultFactory.Wrap() ) {}
		public AssignableDelegatedParameterizedScope( Func<object, T> factory ) : base( factory ) {}
	}

	public class AssignableDelegatedParameterizedScope<TInstance, TValue> : AssignableDelegatedParameterizedSource<TInstance, TValue>
	{
		public AssignableDelegatedParameterizedScope( Func<TInstance, TValue> factory ) : base( new CurrentExecutionScope<Func<TInstance, TValue>>( factory.Self ) ) {}
	}

	public interface IAssignableDelegatedParameterizedSource<TInstance, TValue> : IAssignable<Func<TInstance, TValue>>, IParameterizedSource<TInstance, TValue> { }

	public class AssignableDelegatedParameterizedSource<TInstance, TValue> : DelegatedParameterizedSource<TInstance, TValue>, IAssignableDelegatedParameterizedSource<TInstance, TValue>
	{
		readonly IWritableStore<Func<TInstance, TValue>> store;

		public AssignableDelegatedParameterizedSource( Func<TInstance, TValue> factory ) : this( new FixedStore<Func<TInstance, TValue>>( factory ) ) {}
		public AssignableDelegatedParameterizedSource( IWritableStore<Func<TInstance, TValue>> store ) : base( instance => store.Get()( instance ) )
		{
			this.store = store;
		}

		// public TValue Get( TInstance parameter ) => Value( parameter );
		public override TValue Get( TInstance parameter ) => store.Value( parameter );
		public virtual void Assign( Func<TInstance, TValue> item ) => store.Assign( item );
	}*/


	/*public class FactoryScopeCache<T> : ConfigurableScopedCache<T>
	{
		readonly Func<T> defaultFactory;
		public FactoryScopeCache( Func<T> defaultFactory ) : base( defaultFactory )
		{
			this.defaultFactory = defaultFactory;
		}

		public override T Get( object parameter ) => Contains( parameter ) ? Get( parameter ) : defaultFactory();
	}

	public class ConfigurableScopedCache<T> : ConfigurableCache<T>
	{
		public ConfigurableScopedCache( Func<T> defaultFactory ) : base( new AssignableDelegatedParameterizedScope<object, T>( defaultFactory.Wrap() ) ) {}

		public override void Assign( Func<object, T> item )
		{
			Remove( Execution.Current() );
			base.Assign( item );
		}
	}*/
}