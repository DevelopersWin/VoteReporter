namespace DragonSpark.Runtime
{
	/*public static class Invocation
	{
		public static Delegate GetCurrent() => AmbientStack.GetCurrentItem<Delegate>();

		public static T Create<T>( T @delegate ) where T : class => Invocation<T>.Default.Get( @delegate as Delegate );
	}*//*

	public class Invocation<T> : Cache<Delegate, T> where T : class
	{
		readonly static MethodInfo MethodInfo = typeof(Invocation<T>).GetTypeInfo().GetDeclaredMethod( nameof(Invoke) );

		public static Invocation<T> Default { get; } = new Invocation<T>();

		Invocation() : base( Create ) {}

		static T Create( Delegate inner )
		{
			var parameters = Parameters.Default.Get( inner.GetMethodInfo() );
			var expressions = ImmutableArray.Create<Expression>( Expression.Constant( inner ), Expression.NewArrayInit( typeof(object), parameters ) ).ToArray();
			var call = Expression.Call( null, MethodInfo, expressions );
			var type = inner.GetMethodInfo().ReturnType;
			var convert = type != typeof(void) ? (Expression)Expression.Convert( call, type ) : call;
			var result = Expression.Lambda<T>( convert, parameters ).Compile();
			return result;
		}

		static object Invoke( Delegate target, object[] parameters )
		{
			using ( new AmbientStack<Delegate>.Assignment( target ) )
			{
				var result = target.DynamicInvoke( parameters );
				return result;
			}
		}
	}*/

	/*class Parameters : Cache<MethodBase, ParameterExpression[]>
	{
		public static Parameters Default { get; } = new Parameters();

		Parameters() : base( method => method.GetParameters().Select( info => Expression.Parameter( info.ParameterType, info.Name ) ).ToArray() ) {}
	}*/
	/*public static class Invocation
	{
		public static DelegateWithParameterCache GetCurrent() => DelegateStore.Instance.Value;

		public static T Create<T>( T @delegate )
		{
			
			return Wrapped.Default.Get( @delegate as DelegateWithParameterCache );
			
		}
	}

	public class Wrapped : AttachedProperty<DelegateWithParameterCache, DelegateWithParameterCache>
	{
		public static Wrapped Default { get; } = new Wrapped();

		Wrapped() : base( Create ) {}

		static DelegateWithParameterCache Create( DelegateWithParameterCache inner )
		{
			var parameters = Parameters.Default.Get( inner.GetMethodInfo() );
			var expressions = ImmutableArray.Create<Expression>( Expression.Constant( inner ), Expression.NewArrayInit( typeof(object), parameters ) ).ToArray();
			var call = Expression.Call( null, MethodInfo, expressions );
			var type = inner.GetMethodInfo().ReturnType;
			var convert = type != typeof(void) ? (Expression)Expression.Convert( call, type ) : call;
			var result = Expression.Lambda<T>( convert, parameters ).Compile();
			return result;
		}
		
		static object Invoke( DelegateWithParameterCache target, object[] parameters )
		{
			using ( new AmbientStack<DelegateWithParameterCache>.Assignment( target ) )
			{
				var result = target.DynamicInvoke( parameters );
				return result;
			}
		}
	}

	class DelegateStack : AmbientStack<DelegateWithParameterCache>
	{
		public new static DelegateStack Default { get; } = new DelegateStack();
	}

	interface IDelegateStore : IStore<DelegateWithParameterCache> {}

	class DelegateStore : DelegatedStore<DelegateWithParameterCache>, IDelegateStore
	{
		public static DelegateStore Instance { get; } = new DelegateStore();

		public DelegateStore() : this( DelegateStack.Default ) {}

		public DelegateStore( IStackStore<DelegateWithParameterCache> store ) : base( store.GetCurrentItem ) {}
	}*/

	/*public static class CurrentDelegate
	{
		public static T Get<T>() where T : class => GlobalDelegateContext<T>.Instance.Value.Get( DelegateStore.Instance.Value );
		public static void Set<T>( DelegateWithParameterCache target, T value ) where T : class => GlobalDelegateContext<T>.Instance.Value.Set( target, value );
	}

	/*class Temp<T>
	{
		readonly DelegateWithParameterCache inner;

		public Temp( DelegateWithParameterCache inner ) : this( inner, Factory<T>.Instance.Create( inner ) ) {}

		public Temp( DelegateWithParameterCache inner, T created )
		{
			this.inner = inner;
		}

		public DelegateWithParameterCache Create()
		{
			return null;
		}
	}#1#

	public class EmptyDelegateProperty<T> : EmptyDelegateProperty where T : class
	{
		public static EmptyDelegateProperty<T> Default { get; } = new EmptyDelegateProperty<T>();
		EmptyDelegateProperty() : base( Factory.Instance.ToDelegate() ) {}

		public T GetDirect( MethodInfo parameter ) => Get( parameter ) as T;

		public new class Factory : EmptyDelegateProperty.Factory
		{
			public static Factory Instance { get; } = new Factory();

			Factory() : base( typeof(T) ) {}
		}
	}

	public class EmptyDelegateProperty : Cache<MethodInfo, DelegateWithParameterCache>
	{
		public EmptyDelegateProperty( Func<MethodInfo, DelegateWithParameterCache> factory ) : base( factory ) {}

		public class Factory : FactoryBase<MethodInfo, DelegateWithParameterCache>
		{
			readonly Type compiledType;

			public Factory( Type compiledType )
			{
				this.compiledType = compiledType;
			}

			public override DelegateWithParameterCache Create( MethodInfo parameter )
			{
				var parameters = Parameters.Default.Get( parameter );
				var body = parameter.ReturnType != typeof(void) ? Expression.Default( parameter.ReturnType ) : Expression.Empty();
				var result = Expression.Lambda( compiledType, body, parameters.ToArray() ).Compile();
				return result;
			}
		}
	}

	public static class Elements
	{
		public static MethodInfo Invoke { get; } = typeof(IDelegateInvoker).GetTypeInfo().GetDeclaredMethod( nameof(IDelegateInvoker.Invoke) );
		public static MethodInfo Relay { get; } = typeof(IDelegateRelay).GetTypeInfo().GetDeclaredMethod( nameof(IDelegateRelay.Relay) );

		public static Func<DelegateWithParameterCache, ConstantExpression> Constant { get; } = Expression.Constant;
	}

	public interface IDelegateRelay
	{
		void Relay( DelegateWithParameterCache source, DelegateWithParameterCache destination );
	}

	public class ContextAwareDelegateProperty<T> : ContextAwareDelegateProperty where T : class
	{
		public ContextAwareDelegateProperty( Func<DelegateWithParameterCache, DelegateWithParameterCache> create ) : base( create ) {}

		public T GetDirect( DelegateWithParameterCache parameter ) => Get( parameter ) as T;

		public new class Factory : ContextAwareDelegateProperty.Factory
		{
			public Factory( IDelegateRelay relay ) : base( relay, RelayDelegateProperty<T>.Default.Get, typeof(T) ) {}
		}
	}

	public class ContextAwareDelegateProperty : Cache<DelegateWithParameterCache, DelegateWithParameterCache>
	{
		public ContextAwareDelegateProperty( Func<DelegateWithParameterCache, DelegateWithParameterCache> create ) : base( create ) {}

		public class Factory : TransformerBase<DelegateWithParameterCache>
		{
			readonly IDelegateRelay relay;
			readonly Func<MethodInfo, DelegateWithParameterCache> relaySource;
			readonly Func<CallDelegateExpressionFactory.Parameter, Expression> callSource;
			readonly Type compiledType;

			public Factory( IDelegateRelay relay, Func<MethodInfo, DelegateWithParameterCache> relaySource, Type compiledType ) : this( relay, relaySource, CallDelegateExpressionFactory.Instance.ToDelegate(), compiledType ) {}

			public Factory( IDelegateRelay relay, Func<MethodInfo, DelegateWithParameterCache> relaySource, Func<CallDelegateExpressionFactory.Parameter, Expression> callSource, Type compiledType )
			{
				this.relay = relay;
				this.relaySource = relaySource;
				this.callSource = callSource;
				this.compiledType = compiledType;
			}

			public override DelegateWithParameterCache Create( DelegateWithParameterCache parameter )
			{
				var methodInfo = parameter.GetMethodInfo();
				var relayDelegate = relaySource( methodInfo );
				var relayCall = Expression.Call( Expression.Constant( relay ), Elements.Relay, ImmutableArray.Create( relayDelegate, parameter ).Select( Elements.Constant ).ToArray() );
				//var call = callSource( new CallDelegateExpressionFactory.Parameter( methodInfo, relayDelegate ) );
				// var type = methodInfo.ReturnType;
				// var convert = type != typeof(void) ? (Expression)Expression.Convert( relayCall, type ) : relayCall;
				// var block = Expression.Block( relayCall, call );
				var result = Expression.Lambda( compiledType, Expression.Default( methodInfo.ReturnType ), Parameters.Default.Get( methodInfo ).ToArray() ).Compile();
				return result;
			}
		}
	}

	public class RelayDelegateProperty<T> : RelayDelegateProperty  where T : class
	{
		public static RelayDelegateProperty<T> Default { get; } = new RelayDelegateProperty<T>();

		RelayDelegateProperty() : base( Factory.Instance.ToDelegate() ) {}

		public T GetDirect( MethodInfo parameter ) => Get( parameter ) as T;

		public new class Factory : RelayDelegateProperty.Factory
		{
			public static Factory Instance { get; } = new Factory();
			Factory() : base( EmptyDelegateProperty<T>.Default.Get, typeof(T) ) {}
		}
	}

	public class RelayDelegateProperty : Cache<MethodInfo, DelegateWithParameterCache>
	{
		public RelayDelegateProperty( Func<MethodInfo, DelegateWithParameterCache> factory ) : base( factory ) {}

		public class Factory : FactoryBase<MethodInfo, DelegateWithParameterCache>
		{
			readonly Func<MethodInfo, DelegateWithParameterCache> emptySource;
			readonly Func<CallDelegateExpressionFactory.Parameter, Expression> callSource;
			readonly Type compiledType;

			/*public Factory( Type compiledType ) : this( ContextAwareDelegateInvoker.Instance, compiledType ) {}

			public Factory( IDelegateInvoker invoker, Type compiledType ) : this( invoker, new EmptyDelegateProperty( new Factory( compiledType ).ToDelegate() ), compiledType ) {}#1#

			public Factory( Func<MethodInfo, DelegateWithParameterCache> emptySource, Type compiledType ) : this( emptySource, CallDelegateExpressionFactory.Instance.ToDelegate(), compiledType ) {}

			public Factory( Func<MethodInfo, DelegateWithParameterCache> emptySource, Func<CallDelegateExpressionFactory.Parameter, Expression> callSource, Type compiledType )
			{
				this.emptySource = emptySource;
				this.callSource = callSource;
				this.compiledType = compiledType;
			}

			public override DelegateWithParameterCache Create( MethodInfo parameter )
			{
				var call = callSource( new CallDelegateExpressionFactory.Parameter( parameter, emptySource( parameter ) ) );
				var result = Expression.Lambda( compiledType, call, Parameters.Default.Get( parameter ).ToArray() ).Compile();
				return result;

/*var parameterExpressions = Parameters.Default.Get( parameter );
				var array = Expression.NewArrayInit( typeof(object), parameterExpressions.Select( expression => Expression.TypeAs( expression, typeof(object) ) ) );
				var expressions = ImmutableArray.Create<Expression>( Expression.Constant( empty ), array ).ToArray();
				var call = Expression.Call( Expression.Constant( ContextAwareDelegateInvoker.Instance ), Elements.Invoke, expressions );
				var type = parameter.ReturnType;
				var convert = type != typeof(void) ? (Expression)Expression.Convert( call, type ) : call;
				var result = Expression.Lambda( compiledType, convert, parameterExpressions.ToArray() ).Compile();
				return result;#1#
			}
		}
	}

	public class CallDelegateExpressionFactory : FactoryBase<CallDelegateExpressionFactory.Parameter, Expression>
	{
		public static CallDelegateExpressionFactory Instance { get; } = new CallDelegateExpressionFactory();

		readonly IDelegateInvoker invoker;

		CallDelegateExpressionFactory() : this( ContextAwareDelegateInvoker.Instance ) {}

		public CallDelegateExpressionFactory( IDelegateInvoker invoker )
		{
			this.invoker = invoker;
		}

		public override Expression Create( Parameter parameter )
		{
			var parameterExpressions = Parameters.Default.Get( parameter.Source );
			var array = Expression.NewArrayInit( typeof(object), parameterExpressions.Select( expression => Expression.TypeAs( expression, typeof(object) ) ) );
			var expressions = ImmutableArray.Create<Expression>( Expression.Constant( parameter.DelegateWithParameterCache ), array ).ToArray();
			var call = Expression.Call( Expression.Constant( invoker ), Elements.Invoke, expressions );
			var type = parameter.Source.ReturnType;
			var result = type != typeof(void) ? (Expression)Expression.Convert( call, type ) : call;
			return result;
			/*var method = parameter.GetMethodInfo();
			var array = Expression.NewArrayInit( typeof(object), Parameters.Default.Get( method ).Select( expression => Expression.TypeAs( expression, typeof(object) ) ) );
			var expressions = ImmutableArray.Create<Expression>( Expression.Constant( parameter ), array ).ToArray();
			var call = Expression.Call( Expression.Constant( invoker ), Elements.Invoke, expressions );
			var type = method.ReturnType;
			var convert = type != typeof(void) ? (Expression)Expression.Convert( call, type ) : call;
			return convert;#1#
		}

		public struct Parameter
		{
			public Parameter( MethodInfo source, DelegateWithParameterCache @delegate )
			{
				Source = source;
				DelegateWithParameterCache = @delegate;
			}

			public MethodInfo Source { get; }
			public DelegateWithParameterCache DelegateWithParameterCache { get; }
		}
	}

	/#1#

	public interface IDelegateInvoker
	{
		object Invoke( DelegateWithParameterCache target, object[] arguments );
	}

	class DelegateInvoker : IDelegateInvoker
	{
		public static DelegateInvoker Instance { get; } = new DelegateInvoker();

		public object Invoke( DelegateWithParameterCache target, object[] arguments ) => target.DynamicInvoke( arguments );
	}

	public interface IDelegateContextProvider<T> : IInstanceStore<DelegateWithParameterCache, T> {}

	public class GlobalDelegateContext<T> : DecoratedStore<IDelegateContextProvider<T>> where T : class
	{
		public static GlobalDelegateContext<T> Instance { get; } = new GlobalDelegateContext<T>();

		GlobalDelegateContext() : base( CurrentDelegateContext<T>.Instance ) {}
	}

	class CurrentDelegateContext<T> : ThreadLocalStore<IDelegateContextProvider<T>> where T : class
	{
		public static CurrentDelegateContext<T> Instance { get; } = new CurrentDelegateContext<T>();

		CurrentDelegateContext() : base( DelegateContextProvider<T>.Instance.ToFactory().ToDelegate() ) {}
	}

	class DelegateContextProvider<T> : IDelegateContextProvider<T> where T : class
	{
		public static DelegateContextProvider<T> Instance { get; } = new DelegateContextProvider<T>();

		readonly IAttachedProperty<DelegateWithParameterCache, T> property;

		DelegateContextProvider() : this( new Cache<DelegateWithParameterCache, T>() ) {}

		public DelegateContextProvider( IAttachedProperty<DelegateWithParameterCache, T> property )
		{
			this.property = property;
		}

		public T Get( DelegateWithParameterCache source ) => property.Get( source );

		public void Set( DelegateWithParameterCache source, T value ) => property.Set( source, value );
	}

	class DelegateStack : AmbientStack<DelegateWithParameterCache>
	{
		public new static DelegateStack Default { get; } = new DelegateStack();
	}

	interface IDelegateStore : IStore<DelegateWithParameterCache> {}

	class DelegateStore : DelegatedStore<DelegateWithParameterCache>, IDelegateStore
	{
		public static DelegateStore Instance { get; } = new DelegateStore();

		public DelegateStore() : this( DelegateStack.Default ) {}

		public DelegateStore( IStackStore<DelegateWithParameterCache> store ) : base( store.GetCurrentItem ) {}
	}

	class ContextAwareDelegateInvoker : IDelegateInvoker
	{
		public static ContextAwareDelegateInvoker Instance { get; } = new ContextAwareDelegateInvoker();

		readonly IDelegateInvoker invoker;
		readonly IStackStore<DelegateWithParameterCache> store;

		ContextAwareDelegateInvoker() : this( DelegateInvoker.Instance ) {}

		public ContextAwareDelegateInvoker( IDelegateInvoker invoker ) : this( invoker, DelegateStack.Default ) {}

		public ContextAwareDelegateInvoker( IDelegateInvoker invoker, IStackStore<DelegateWithParameterCache> store )
		{
			this.invoker = invoker;
			this.store = store;
		}

		public object Invoke( DelegateWithParameterCache target, object[] arguments )
		{
			using ( new DelegateStack.Assignment( store, target ) )
			{
				return invoker.Invoke( target, arguments );
			}
		}
	}

	class Parameters : AttachedProperty<MethodBase, ImmutableArray<ParameterExpression>>
	{
		public static Parameters Default { get; } = new Parameters();

		Parameters() : base( method => method.GetParameters().ToImmutableArray().Select( info => Expression.Parameter( info.ParameterType, info.Name ) ).ToImmutableArray() ) {}
	}*/
}
