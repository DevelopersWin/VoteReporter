using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.Runtime.Sources
{
	public static class Factory
	{
		public static T Self<T>( this T @this ) => @this;

		public static Func<T> For<T>( T @this ) => ( typeof(T).GetTypeInfo().IsValueType ? new Source<T>( @this ) : @this.Sourced() ).Get;

		public static Func<T> Fix<T>( this ISource<T> @this ) => new Func<T>( @this.Get ).Fix();
		public static Func<T> Fix<T>( this Func<T> @this ) => FixedDelegateBuilder<T>.Instance.Get( @this );
		public static Func<TParameter, TResult> Fix<TParameter, TResult>( this Func<TParameter, TResult> @this ) => CacheFactory.Create( @this ).Get;

		public static Func<object, T> Scope<T>( this Func<T> @this ) => @this.Wrap().Fix();

		public static Func<object, Func<TParameter, TResult>> Scope<TParameter, TResult>( this Func<TParameter, TResult> @this ) => new Cache<TParameter, TResult>( @this ).Get;
		class Cache<TParameter, TResult> : FactoryCache<Func<TParameter, TResult>>
		{
			readonly Func<TParameter, TResult> factory;

			public Cache( Func<TParameter, TResult> factory )
			{
				this.factory = factory;
			}

			protected override Func<TParameter, TResult> Create( object parameter ) => CacheFactory.Create( factory ).Get;
		}
	}

	public sealed class FixedDelegateBuilder<T> : TransformerBase<Func<T>>
	{
		public static IParameterizedSource<Func<T>, Func<T>> Instance { get; } = new FixedDelegateBuilder<T>()/*.ToCache()*/;
		FixedDelegateBuilder() {}

		public override Func<T> Get( Func<T> parameter ) => new FixedDeferedSource<T>( parameter ).Get;
	}

	public class ScopeContext : FixedSource<object>
	{
		readonly Func<object> defaultScope;

		public ScopeContext() : this( Execution.Context ) {}

		public ScopeContext( ISource<ISource> defaultScope ) : this( defaultScope.Delegate() ) {}

		public ScopeContext( Func<object> defaultScope )
		{
			this.defaultScope = defaultScope;
		}

		public override object Get() => SourceCoercer<object>.Instance.Coerce( base.Get() ) ?? defaultScope();
	}

	public interface IScopeAware : IAssignable<ISource> {}
	public interface IScopeAware<in T> : IScopeAware, IAssignable<Func<object, T>>, IAssignable<Func<T>> {}

	public interface IScope<T> : ISource<T>, IScopeAware<T> {}

	public class Scope<T> : SourceBase<T>, IScope<T>
	{
		readonly ICache<Func<object, T>> factories = new Cache<Func<object, T>>();
		readonly IAssignableSource<object> scope;
		readonly IAssignableSource<Func<object, T>> defaultFactory = new FixedSource<Func<object, T>>();

		public Scope() : this( () => default(T) ) {}

		public Scope( Func<T> defaultFactory ) : this( defaultFactory.Wrap() ) {}

		public Scope( Func<object, T> defaultFactory ) : this( new ScopeContext(), defaultFactory ) {}

		protected Scope( IAssignableSource<object> scope, Func<object, T> defaultFactory )
		{
			this.scope = scope;
			this.defaultFactory.Assign( defaultFactory );
		}

		public virtual void Assign( [Optional]Func<T> item ) => factories.SetOrClear( scope.Get(), item?.Wrap() );

		public virtual void Assign( Func<object, T> item )
		{
			defaultFactory.Assign( item );

			factories.Remove( scope.Get() );
		}

		public override T Get()
		{
			var context = scope.Get();
			var factory = factories.Get( context ) ?? defaultFactory.Get();
			var result = factory( context );
			return result;
		}

		public void Assign( ISource item ) => scope.Assign( item );
	}
}