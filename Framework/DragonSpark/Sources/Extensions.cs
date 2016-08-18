using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Sources
{
	public static class Extensions
	{
		public static ICommand Configured<T>( this IAssignable<T> @this, T value ) => new AssignCommand<T>( @this ).Fixed( value );

		public static void Assign<TParameter, TResult>( this IParameterizedScope<TParameter, TResult> @this, Func<TParameter, TResult> instance ) => @this.Assign( instance.Self );

		public static void Assign<T>( this IScopeAware<T> @this, T instance ) => @this.Assign( Factory.For( instance ) );

		public static T WithInstance<T>( this IScope<T> @this, T instance )
		{
			@this.Assign( instance );
			return @this.Get();
		}

		public static T WithFactory<T>( this IScope<T> @this, Func<T> factory )
		{
			@this.Assign( factory );
			return @this.Get();
		}

		public static IEnumerable<T> Get<T>( this IEnumerable<ISource<T>> @this ) => @this.Select( source => source.Get() );

		public static T ScopedWithDefault<T>( this T @this ) where T : IScopeAware => @this.ScopedWith( ExecutionContext.Instance );

		public static T ScopedWith<T>( this T @this, ISource scope ) where T : IScopeAware
		{
			@this.Assign( scope );
			return @this;
		}

		public static ICommand Configured<T>( this IAssignable<Func<T>> @this, ISource<T> source ) => @this.Configured( source.ToDelegate() );
		public static ICommand Configured<T>( this IAssignable<Func<T>> @this, T instance ) => @this.Configured( Factory.For( instance ) );
		public static ICommand Configured<T>( this IAssignable<Func<T>> @this, Func<T> factory ) => new AssignCommand<Func<T>>( @this ).Fixed( factory );
		public static ICommand Configured<T>( this IAssignable<Func<object, T>> @this, Func<object, T> factory ) => new AssignCommand<Func<object, T>>( @this ).Fixed( factory );

		public static Func<TParameter, TResult> Delegate<TParameter, TResult>( this ISource<IParameterizedSource<TParameter, TResult>> @this ) => SourceDelegates<TParameter, TResult>.Default.Get( @this );
		class SourceDelegates<TParameter, TResult> : Cache<ISource<IParameterizedSource<TParameter, TResult>>, Func<TParameter, TResult>>
		{
			public static SourceDelegates<TParameter, TResult> Default { get; } = new SourceDelegates<TParameter, TResult>();
			SourceDelegates() : base( source => new Factory( source ).Get ) {}

			sealed class Factory : ParameterizedSourceBase<TParameter, TResult>
			{
				readonly ISource<IParameterizedSource<TParameter, TResult>> source;
				public Factory( ISource<IParameterizedSource<TParameter, TResult>> source )
				{
					this.source = source;
				}

				public override TResult Get( TParameter parameter ) => source.Get().Get( parameter );
			}
		}

		public static Func<object> Delegate( this ISource<ISource> @this ) => @this.ToDelegate().Delegate();
		public static Func<object> Delegate( this Func<ISource> @this ) => Delegates.Default.Get( @this );
		class Delegates : Cache<Func<ISource>, Func<object>>
		{
			public static Delegates Default { get; } = new Delegates();
			Delegates() : base( source => new Factory( source ).Get ) {}

			class Factory : SourceBase<object>
			{
				readonly Func<ISource> source;
				public Factory( Func<ISource> source )
				{
					this.source = source;
				}

				public override object Get() => source().Get();
			}
		}

		public static Func<T> Delegate<T>( this ISource<ISource<T>> @this ) => @this.ToDelegate().Delegate();
		public static Func<T> Delegate<T>( this Func<ISource<T>> @this ) => Delegates<T>.Default.Get( @this );
		class Delegates<T> : Cache<Func<ISource<T>>, Func<T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() : base( source => new Factory( source ).Get ) {}

			class Factory : SourceBase<T>
			{
				readonly Func<ISource<T>> source;
				public Factory( Func<ISource<T>> source )
				{
					this.source = source;
				}

				public override T Get() => source().Get();
			}
		}

		public static IScope<T> ToScope<T>( this IParameterizedSource<object, T> @this ) => @this.ToSourceDelegate().ToScope();
		public static IScope<T> ToScope<T>( this Func<object, T> @this ) => Scopes<T>.Default.Get( @this );

		class Scopes<T> : Cache<Func<object, T>, IScope<T>>
		{
			public static Scopes<T> Default { get; } = new Scopes<T>();
			Scopes() : base( cache => new Scope<T>( cache.Fix<object, T>() ) ) {}
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
}