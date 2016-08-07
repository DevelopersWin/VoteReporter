using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Sources;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime
{
	public interface IAssignable
	{
		void Assign( object item );
	}

	public interface IAssignable<in T> /*: IAssignable*/
	{
		void Assign( T item );
	}

	public interface IAssignableSource<T> : ISource<T>, IAssignable<T> {}

	public interface ISource<out T> : ISource
	{
		new T Get();
	}

	public interface ISource
	{
		object Get();
	}

	public static class Source
	{
		static IGenericMethodContext<Invoke> Methods { get; } = typeof(Source).Adapt().GenericFactoryMethods[nameof(Empty)];

		public static ISource Empty( Type type ) => Methods.Make( type ).Invoke<ISource>();

		public static ISource<T> Empty<T>() => EmptySource<T>.Instance;

		public static ISource<T> Sourced<T>( this T @this ) => Support<T>.Sources.Get( @this );

		static class Support<T>
		{
			public static ICache<T, ISource<T>> Sources { get; } = CacheFactory.Create<T, ISource<T>>( arg => new Source<T>( arg ) );
		}

		/*public static IParameterizedSource<TParameter, TResult> ToFactory<TParameter, TResult>( this Func<TParameter, TResult> @this ) => ParameterizedSources<TParameter, TResult>.Default.Get( @this );
		// public static IFactory<TParameter, TResult> ToFactory<TParameter, TResult>( this TResult @this ) where TResult : class => ParameterizedSources<TResult>.Default.Get( @this ).Wrap<TParameter, TResult>();
		class ParameterizedSources<TParameter, TResult> : Cache<Func<TParameter, TResult>, IParameterizedSource<TParameter, TResult>>
		{
			public static ParameterizedSources<TParameter, TResult> Default { get; } = new ParameterizedSources<TParameter, TResult>();
			ParameterizedSources() : base( result => new DelegatedFactory<TParameter, TResult>( result ) ) {}
		}*/
	}

	public class FixedDeferedSource<T> : SourceBase<T>
	{
		readonly Lazy<T> lazy;

		public FixedDeferedSource( Func<T> factory ) : this( new Lazy<T>( factory ) ) {}

		public FixedDeferedSource( Lazy<T> lazy )
		{
			this.lazy = lazy;
		}

		public override T Get() => lazy.Value;
	}

	/*[ReaderWriterSynchronized]
	public class SynchronizedFixedSource<T> : FixedSource<T>
	{
		[Reader]
		public override T Get() => base.Get();

		[Writer]
		protected override void OnAssign( T item ) => base.OnAssign( item );
	}*/

	public class FixedSource<T> : AssignableSourceBase<T>
	{
		T reference;

		public FixedSource() {}

		public FixedSource( T reference )
		{
			Assign( reference );
		}

		public sealed override void Assign( T item ) => OnAssign( item );

		protected virtual void OnAssign( T item ) => reference = item;

		public override T Get() => reference;

		// protected override void OnDispose() => reference = default(T);
	}

	public abstract class AssignableSourceBase<T> : SourceBase<T>, IAssignableSource<T>
	{
		public abstract void Assign( T item );
	}

	public abstract class SourceBase<T> : ISource<T>
	{
		public abstract T Get();

		object ISource.Get() => Get();
	}

	public class DelegatedSource<T> : SourceBase<T>
	{
		readonly Func<T> get;

		public DelegatedSource( Func<T> get )
		{
			this.get = get;
		}

		public override T Get() => get();
	}

	public class EmptySource<T> : Source<T>
	{
		public static EmptySource<T> Instance { get; } = new EmptySource<T>();
		EmptySource() : base( default(T) ) {}
	}

	public class Source<T> : SourceBase<T>
	{
		readonly T instance;

		public Source( T instance )
		{
			this.instance = instance;
		}

		public override T Get() => instance;
	}

	public static class SourceExtensions
	{
		public static Func<TParameter, TResult> Delegate<TParameter, TResult>( this ISource<IParameterizedSource<TParameter, TResult>> @this ) => SourceDelegates<TParameter, TResult>.Default.Get( @this );
		class SourceDelegates<TParameter, TResult> : Cache<ISource<IParameterizedSource<TParameter, TResult>>, Func<TParameter, TResult>>
		{
			public static SourceDelegates<TParameter, TResult> Default { get; } = new SourceDelegates<TParameter, TResult>();
			SourceDelegates() : base( source => new Factory( source ).Create ) {}

			class Factory : FactoryBase<TParameter, TResult>
			{
				readonly ISource<IParameterizedSource<TParameter, TResult>> source;
				public Factory( ISource<IParameterizedSource<TParameter, TResult>> source )
				{
					this.source = source;
				}

				public override TResult Create( TParameter parameter ) => source.Get().Get( parameter );
			}
		}

		public static Func<object> Delegate( this ISource<ISource> @this ) => @this.ToDelegate().Delegate();
		public static Func<object> Delegate( this Func<ISource> @this ) => Delegates.Default.Get( @this );
		class Delegates : Cache<Func<ISource>, Func<object>>
		{
			public static Delegates Default { get; } = new Delegates();
			Delegates() : base( source => new Factory( source ).Create ) {}

			class Factory : FactoryBase<object>
			{
				readonly Func<ISource> source;
				public Factory( Func<ISource> source )
				{
					this.source = source;
				}

				public override object Create() => source().Get();
			}
		}

		public static Func<T> Delegate<T>( this ISource<ISource<T>> @this ) => @this.ToDelegate().Delegate();
		public static Func<T> Delegate<T>( this Func<ISource<T>> @this ) => Delegates<T>.Default.Get( @this );
		class Delegates<T> : Cache<Func<ISource<T>>, Func<T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() : base( source => new Factory( source ).Create ) {}

			class Factory : FactoryBase<T>
			{
				readonly Func<ISource<T>> source;
				public Factory( Func<ISource<T>> source )
				{
					this.source = source;
				}

				public override T Create() => source().Get();
			}
		}

		/*public static Func<T> Delegate<T>( this ISource<ISource<T>> @this ) => @this.ToDelegate().Delegate();
		class SourceDelegates<T> : Cache<ISource<ISource<T>>, Func<T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			SourceDelegates() : base( source => new Factory( source ).Create ) {}

			class Factory : FactoryBase<T>
			{
				readonly ISource<ISource<T>> source;
				public Factory( ISource<ISource<T>> source )
				{
					this.source = source;
				}

				public override T Create() => source.Get().Get();
			}
		}*/
	}

	public interface IParameterizedSource<out T> : IParameterizedSource<object, T> {}

	public interface IParameterizedSource<in TParameter, out TResult> : IParameterizedSource
	{
		TResult Get( TParameter parameter );
	}

	public interface IAssignableParameterizedSource<T> : IAssignableParameterizedSource<object, T>, IParameterizedSource<T> {}

	public interface IAssignableParameterizedSource<in TParameter, TResult> : IParameterizedSource<TParameter, TResult>
	{
		void Set( TParameter parameter, TResult result );
	}

	public abstract class AssignableParameterizedSourceBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IAssignableParameterizedSource<TParameter, TResult>
	{
		public abstract void Set( TParameter parameter, TResult result );
	}

	public class DecoratedAssignableParameterizedSource<TParameter, TResult> : DecoratedParameterizedSource<TParameter, TResult>, IAssignableParameterizedSource<TParameter, TResult>
	{
		readonly IAssignableParameterizedSource<TParameter, TResult> source;
		public DecoratedAssignableParameterizedSource( IAssignableParameterizedSource<TParameter, TResult> source ) : base( source )
		{
			this.source = source;
		}

		public void Set( TParameter parameter, TResult result ) => source.Set( parameter, result );
	}

	public class DecoratedParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>
	{
		public DecoratedParameterizedSource( IParameterizedSource<TParameter, TResult> source ) : base( source.Get ) {}
	}

	public class DelegatedParameterizedSource<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> source;

		public DelegatedParameterizedSource( Func<TParameter, TResult> source )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source( parameter );
	}

	public class SourcedParameterizedSource<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly ISource<Func<TParameter, TResult>> source;

		public SourcedParameterizedSource( ISource<Func<TParameter, TResult>> source )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source.Get()( parameter );
	}

	public abstract class ParameterizedSourceBase<T> : ParameterizedSourceBase<object, T>, IParameterizedSource<T> {}

	public abstract class ParameterizedSourceBase<TParameter, TResult> : IParameterizedSource<TParameter, TResult>
	{
		public abstract TResult Get( TParameter parameter );

		object IParameterizedSource.Get( object parameter ) => parameter is TParameter ? Get( (TParameter)parameter ) : default(TResult);
	}

	public interface IParameterizedSource
	{
		object Get( object parameter );
	}

	/*public class FactoryParameterizedScope<T> : FactoryParameterizedScope<object, T>, IParameterizedScope<T>
	{
		public FactoryParameterizedScope( Func<object, T> source ) : base( source ) {}
	}

	public class FactoryParameterizedScope<TParameter, TResult> : ParameterizedScopeBase<TParameter, TResult>
	{
		public FactoryParameterizedScope( Func<TParameter, TResult> source ) : base( source ) {}
	}*/

	public class CachedParameterizedScope<T> : CachedParameterizedScope<object, T>, IParameterizedScope<T>
	{
		public CachedParameterizedScope( Func<object, T> source ) : base( source ) {}
	}

	public class CachedParameterizedScope<TParameter, TResult> : ParameterizedScope<TParameter, TResult>
	{
		static Func<TParameter, TResult> Create( Func<TParameter, TResult> parameter ) => CacheFactory.Create( parameter ).Get;

		public CachedParameterizedScope( Func<TParameter, TResult> source ) : base( new Scope<Func<TParameter, TResult>>( new FixedFactory<Func<TParameter, TResult>, Func<TParameter, TResult>>( Create, source ).Create ) ) {}

		public override void Assign( Func<Func<TParameter, TResult>> item ) => base.Assign( new FixedFactory<Func<TParameter, TResult>, Func<TParameter, TResult>>( Create, item() ).Create );
	}

	public interface IParameterizedScope<T> : IParameterizedScope<object, T>, IParameterizedSource<T> {}
	public interface IParameterizedScope<TParameter, TResult> : IParameterizedSource<TParameter, TResult>, IScopeAware<Func<TParameter, TResult>> {}

	public class ParameterizedScope<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IParameterizedScope<TParameter, TResult>
	{
		readonly IScope<Func<TParameter, TResult>> scope;

		public ParameterizedScope( Func<TParameter, TResult> source ) : this( new Scope<Func<TParameter, TResult>>( source.Wrap() ) ) {}

		protected ParameterizedScope( IScope<Func<TParameter, TResult>> scope )
		{
			this.scope = scope;
		}

		public override TResult Get( TParameter key ) => scope.Get().Invoke( key );

		//public virtual void Assign( Func<TParameter, TResult> item ) => scope.Assign( item );
		public void Assign( ISource item ) => scope.Assign( item );

		public virtual void Assign( Func<object, Func<TParameter, TResult>> item ) => scope.Assign( item );
		public virtual void Assign( Func<Func<TParameter, TResult>> item ) => scope.Assign( item );
	}

	// public interface IDelegateScope<T> : IScope<Func<T>>, IAssignable<Func<T>>/*, ISource<T>*/ {}
	/*public class DelegateScope<T> : Scope<Func<T>>, IDelegateScope<T>
	{
		public DelegateScope( Func<T> source ) : base( source.Self ) {}
		public DelegateScope( Func<Func<T>> source ) : base( source ) {}

		// public new T Get() => base.Get().Invoke();
	}*/

	public class CachedScope<T> : Scope<T>
	{
		public CachedScope() : this( () => default(T) ) {}

		public CachedScope( Func<T> source ) : this( source.Wrap() ) {}

		public CachedScope( Func<object, T> defaultFactory ) : base( defaultFactory.Fix() ) {}

		public override void Assign( Func<object, T> item ) => base.Assign( item.Fix() );
	}
}
