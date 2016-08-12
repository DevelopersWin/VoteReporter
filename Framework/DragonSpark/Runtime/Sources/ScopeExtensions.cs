using DragonSpark.Activation;
using DragonSpark.Extensions;
using System;
using System.Windows.Input;

namespace DragonSpark.Runtime.Sources
{
	public static class Extensions
	{
		public static ICommand Configured<T>( this IAssignable<T> @this, T value ) => new AssignCommand<T>( @this ).Fixed( value );

		public static T Get<T>( this IStore<T> @this ) => @this.Value;

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
	}
}