using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;
using System.Runtime.InteropServices;
using ICommand = System.Windows.Input.ICommand;

namespace DragonSpark.Commands
{
	public static class Extensions
	{
		/*public static IParameterizedSource<object, TResult> Apply<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, ICoercer<TParameter> coercer ) => Apply( @this.ToSourceDelegate(), coercer.ToDelegate() );
		public static IParameterizedSource<object, TResult> Apply<TParameter, TResult>( this Func<TParameter, TResult> @this, Coerce<TParameter> coerce ) =>
			new CoercedParameterizedSource<TParameter, TResult>( coerce, @this );
		public static IParameterizedSource<TFrom, TResult> Apply<TFrom, TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, ICoercer<TFrom, TParameter> coercer ) => Apply( @this.ToSourceDelegate(), coercer.ToDelegate() );
		public static IParameterizedSource<TFrom, TResult> Apply<TFrom, TParameter, TResult>( this Func<TParameter, TResult> @this, Func<TFrom, TParameter> coerce ) =>
			new CoercedParameterizedSource<TFrom, TParameter, TResult>( coerce, @this );

		public static IParameterizedSource<TParameter, TResult> Apply<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, IAlteration<TParameter> alteration ) => Apply( @this, alteration.ToDelegate() );
		public static IParameterizedSource<TParameter, TResult> Apply<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, Alter<TParameter> alter ) => Apply( @this.ToSourceDelegate(), alter );
		public static IParameterizedSource<TParameter, TResult> Apply<TParameter, TResult>( this Func<TParameter, TResult> @this, Alter<TParameter> alter ) =>
			new AlteredParameterizedSource<TParameter, TResult>( alter, @this );

		public static IParameterizedSource<TParameter, TResult> Apply<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, IAlteration<TResult> selector ) => Apply( @this, selector.ToDelegate() );
		public static IParameterizedSource<TParameter, TResult> Apply<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, Alter<TResult> selector ) => Apply( @this.ToSourceDelegate(), selector );
		public static IParameterizedSource<TParameter, TResult> Apply<TParameter, TResult>( this Func<TParameter, TResult> @this, Alter<TResult> selector ) =>
			new AlteredResultParameterizedSource<TParameter, TResult>( selector, @this );*/

		public static ICommand<T> Apply<T>( this ICommand<T> @this, ISpecification<T> specification ) => new SpecificationCommand<T>( specification, @this.ToDelegate() );


		public static ICommand<T> Cast<T>( this ICommand @this ) => Cast<T>( @this, arg => arg );

		public static ICommand<T> Cast<T>( this ICommand @this, Func<T, object> projection ) => new ProjectedCommand<T>( @this, projection );

		// public static void Run( this IRunCommand @this ) => @this.Execute( default(object) );

		public static Action ToRunDelegate( this IRunCommand @this ) => RunDelegates.Default.Get( @this );
		sealed class RunDelegates : Cache<IRunCommand, Action>
		{
			public static RunDelegates Default { get; } = new RunDelegates();
			RunDelegates() : base( command => command.Execute ) {}
		}

		// public static void Run<T>( this ICommand<T> @this ) => @this.Execute( default(T) );

		public static TCommand Run<TCommand, TParameter>( this TCommand @this, TParameter parameter ) where TCommand : ICommand<TParameter>
		{
			var result = @this.CanExecute( parameter ) ? @this : default(TCommand);
			result?.Execute( parameter );
			return result;
		}

		// public static FixedCommand<Func<T>> Fixed<T>( this ICommand<Func<T>> @this, T parameter ) => new FixedCommand<Func<T>>( @this, new FixedFactory<T>( parameter ).Create );
		public static SuppliedCommand<T> Fixed<T>( this ICommand<T> @this, [Optional]T parameter ) => new SuppliedCommand<T>( @this, parameter );
		public static DeferredCommand<T> Fixed<T>( this ICommand<T> @this, Func<T> parameter ) => new DeferredCommand<T>( @this, parameter );

		public static Action<T> ToDelegate<T>( this ICommand<T> @this ) => DelegateCache<T>.Default.Get( @this );
		sealed class DelegateCache<T> : Cache<ICommand<T>, Action<T>>
		{
			public static DelegateCache<T> Default { get; } = new DelegateCache<T>();
			DelegateCache() : base( command => command.Execute ) {}
		}

		public static IAlteration<T> ToAlteration<T>( this ICommand<T> @this ) => Alterations<T>.Default.Get( @this );
		sealed class Alterations<T> : Cache<ICommand<T>, IAlteration<T>>
		{
			public static Alterations<T> Default { get; } = new Alterations<T>();
			Alterations() : base( command => new ConfiguringAlteration<T>( command.ToDelegate() ) ) {}
		}

		/*public static ICommand<T> WithAutoValidation<T>( this ICommand<T> @this ) => AutoValidationCache<T>.Default.Get( @this );
		class AutoValidationCache<T> : Cache<ICommand<T>, ICommand<T>>
		{
			public static AutoValidationCache<T> Default { get; } = new AutoValidationCache<T>();

			AutoValidationCache() : base( command => new AutoValidatingCommand<T>( command ) ) {}
		}*/

		/*public static Action<object> ToDelegate( this ICommand @this ) => DelegateCache.Default.Get( @this );
		class DelegateCache : Cache<ICommand, Action<object>>
		{
			public static DelegateCache Default { get; } = new DelegateCache();

			DelegateCache() : base( command => command.Execute ) {}
		}*/

		public static ISpecification<object> ToSpecification( this ICommand @this ) => SpecificationCache.Default.Get( @this );
		class SpecificationCache : Cache<ICommand, ISpecification<object>>
		{
			public static SpecificationCache Default { get; } = new SpecificationCache();
			SpecificationCache() : base( command => new DelegatedSpecification<object>( command.CanExecute ) ) {}
		}

		public static ISpecification<T> ToSpecification<T>( this ICommand<T> @this ) => SpecificationCache<T>.Default.Get( @this );
		class SpecificationCache<T> : Cache<ICommand<T>, ISpecification<T>>
		{
			public static SpecificationCache<T> Default { get; } = new SpecificationCache<T>();
			SpecificationCache() : base( command => new DelegatedSpecification<T>( command.IsSatisfiedBy ) ) {}
		}

		public static Action<T> Wrap<T>( this Action @this ) => Wrappers<T>.Default.Get( @this );
		sealed class Wrappers<T> : Cache<Action, Action<T>>
		{
			public static Wrappers<T> Default { get; } = new Wrappers<T>();
			Wrappers() : base( result => new Wrapper<T>( result ).Execute ) {}
		}
		sealed class Wrapper<T> : CommandBase<T>
		{
			readonly Action action;

			public Wrapper( Action action )
			{
				this.action = action;
			}

			public override void Execute( T parameter ) => action();
		}

		public static Action<T> Timed<T>( this ICommand<T> @this ) => @this.ToDelegate().Timed();
		public static Action<T> Timed<T>( this Action<T> @this ) => Timed( @this, Sources.Defaults.ParameterizedTimerTemplate );
		public static Action<T> Timed<T>( this Action<T> @this, string template ) => new TimedDelegatedCommand<T>( @this, template ).Execute;
	}
}
