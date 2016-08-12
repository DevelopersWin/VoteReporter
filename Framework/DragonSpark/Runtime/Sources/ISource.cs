using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Runtime.Sources
{
	public interface IAssignable<in T>
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

		protected override void OnDispose() => reference = default(T);
	}

	public class SourceCollection<TStore, TInstance> : CollectionBase<TStore> where TStore : ISource<TInstance>
	{
		public SourceCollection() {}
		public SourceCollection( IEnumerable<TStore> items ) : base( items ) {}
		public SourceCollection( ICollection<TStore> source ) : base( source ) {}

		public ImmutableArray<TInstance> Instances() => Query.Select( entry => entry.Get() ).ToImmutableArray();
	}

	public abstract class AssignableSourceBase<T> : SourceBase<T>, IAssignableSource<T>, IDisposable
	{
		public abstract void Assign( T item );

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		protected virtual void OnDispose() {}
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

	public interface IParameterizedScope<T> : IParameterizedScope<object, T>, IParameterizedSource<T> {}
	public interface IParameterizedScope<TParameter, TResult> : IParameterizedSource<TParameter, TResult>, IScopeAware<Func<TParameter, TResult>> {}

	public class ParameterizedScope<T> : ParameterizedScope<object, T>, IParameterizedScope<T>
	{
		public ParameterizedScope( Func<object, T> source ) : base( source ) {}
		public ParameterizedScope( Func<object, Func<object, T>> source ) : base( source ) {}
		protected ParameterizedScope( IScope<Func<object, T>> scope ) : base( scope ) {}
	}

	public class ParameterizedScope<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IParameterizedScope<TParameter, TResult>
	{
		readonly IScope<Func<TParameter, TResult>> scope;

		public ParameterizedScope( Func<TParameter, TResult> source ) : this( source.Wrap() ) {}

		public ParameterizedScope( Func<object, Func<TParameter, TResult>> source ) : this( new Scope<Func<TParameter, TResult>>( source ) ) {}

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
}