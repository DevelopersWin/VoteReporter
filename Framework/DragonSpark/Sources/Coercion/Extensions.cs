using DragonSpark.Commands;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Sources.Coercion
{
	public static class Extensions
	{
		public static CoercedCommand<TFrom, TParameter> Allow<TFrom, TParameter>( this ICommand<TParameter> @this, IParameterizedSource<TFrom, TParameter> coercer ) => @this.ToDelegate().Allow( coercer );
		public static CoercedCommand<TFrom, TParameter> Allow<TFrom, TParameter>( this Action<TParameter> @this, IParameterizedSource<TFrom, TParameter> coercer ) => new CoercedCommand<TFrom,TParameter>( coercer, @this );

		public static ISource<TResult> Into<TParameter, TResult>( this ISource<TParameter> @this, IParameterizedSource<TParameter, TResult> coerce ) => @this.Into( coerce.ToDelegate() );
		public static ISource<TResult> Into<TParameter, TResult>( this ISource<TParameter> @this, Func<TParameter, TResult> coerce ) => @this.ToDelegate().Into( coerce );
		public static ISource<TResult> Into<TParameter, TResult>( this Func<TParameter> @this, Func<TParameter, TResult> coerce ) => coerce.WithParameter( @this );

		public static IParameterizedSource<TFrom, TResult> Allow<TFrom, TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, IParameterizedSource<TFrom, TParameter> coerce ) => @this.ToDelegate().Allow( coerce.ToDelegate() );
		public static IParameterizedSource<TFrom, TResult> Allow<TFrom, TParameter, TResult>( this Func<TParameter, TResult> @this, IParameterizedSource<TFrom, TParameter> coerce ) => @this.Allow( coerce.ToDelegate() );
		public static IParameterizedSource<TFrom, TResult> Allow<TFrom, TParameter, TResult>( this Func<TParameter, TResult> @this, Func<TFrom, TParameter> coerce )
			=> new ParameterCoercionSource<TFrom, TParameter, TResult>( coerce, @this );

		public static IParameterizedSource<object, TResult> Cast<TResult, TTo>( this IParameterizedSource<object, TResult> @this ) => @this.ToDelegate().Cast<TResult, TTo>();
		public static IParameterizedSource<object, TResult> Cast<TResult, TTo>( this Func<object, TResult> @this ) => 
			new ResultCoercionSource<object, TResult, TTo>( @this, ValidatedCastCoercer<TResult, TTo>.Default.ToDelegate() );

		public static Func<TParameter, TResult> GetDelegate<TFrom, TParameter, TResult>( this IParameterizedSource<TFrom, IParameterizedSource<TParameter, TResult>> @this, TFrom parameter ) => @this.To( DelegateCoercer<TParameter, TResult>.Default ).Get( parameter );
		public static IParameterizedSource<TParameter, TTo> To<TParameter, TResult, TTo>( this IParameterizedSource<TParameter, TResult> @this, IParameterizedSource<TResult, TTo> coerce ) => @this.To( coerce.ToDelegate() );
		public static IParameterizedSource<TParameter, TTo> To<TParameter, TResult, TTo>( this IParameterizedSource<TParameter, TResult> @this, Func<TResult, TTo> coerce ) => @this.ToDelegate().To( coerce );
		public static IParameterizedSource<TParameter, TTo> To<TParameter, TResult, TTo>( this Func<TParameter, TResult> @this, IParameterizedSource<TResult, TTo> coerce ) => @this.To( coerce.ToDelegate() );
		public static IParameterizedSource<TParameter, TTo> To<TParameter, TResult, TTo>( this Func<TParameter, TResult> @this, Func<TResult, TTo> coerce )
			=> new ResultCoercionSource<TParameter, TResult, TTo>( @this, coerce );
	}

	public sealed class DelegateCoercer<TParameter, TResult> : ParameterizedSourceBase<IParameterizedSource<TParameter, TResult>, Func<TParameter, TResult>>
	{
		public static DelegateCoercer<TParameter, TResult> Default { get; } = new DelegateCoercer<TParameter, TResult>();
		DelegateCoercer() {}
		public override Func<TParameter, TResult> Get( IParameterizedSource<TParameter, TResult> parameter ) => parameter.ToDelegate();
	}
}
