using DragonSpark.Activation;
using DragonSpark.Extensions;
using System;
using System.Windows.Input;

namespace DragonSpark.Runtime.Sources
{
	public static class ScopeExtensions
	{
		// public static void Assign<T>( this IScope<T> @this, T factory ) => @this.Assign( Source.For( factory ) );

		public static void Assign<TParameter, TResult>( this IParameterizedScope<TParameter, TResult> @this, Func<TParameter, TResult> instance ) => @this.Assign( instance.Self );

		public static void Assign<T>( this IScopeAware<T> @this, T instance ) => @this.Assign( Factory.For( instance ) );

		//public static void Assign<T>( this IScope<T> @this, Func<T> factory ) => @this.Assign( factory.Wrap() );

		public static T Assigned<T>( this IScope<T> @this, T instance )
		{
			@this.Assign( instance );
			return @this.Get();
		}

		public static T Assigned<T>( this IScope<T> @this, Func<T> factory )
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

		//public static ICommand From<T>( this IScope<T> @this, T value ) => new ConfigureGlobalScopeCommand<T>( value, @this );

		//public static ICommand From<T>( this IParameterizedScope<object, T> @this, T value ) => new ConfigureParameterizedScopeCommand<T>( value, @this );

		// public static ICommand ConfigureGlobal<T>( this IParameterizedScope<object, T> @this, T instance ) => new AssignCommand<Func<object, T>>( @this ).Fixed( Delegates.For( instance ).Wrap() );
		public static ICommand Configured<T>( this IParameterizedScope<T> @this, T instance ) => @this.Configured<object, T>( instance );
		public static ICommand Configured<TParameter, TResult>( this IParameterizedScope<TParameter, TResult> @this, TResult instance )
			=> new AssignCommand<Func<Func<TParameter, TResult>>>( @this ).Fixed( instance.Wrap<TParameter, TResult>().Self );

		public static ICommand Configured<T>( this IAssignable<Func<T>> @this, ISource<T> source ) => @this.Configured( source.ToDelegate() );
		public static ICommand Configured<T>( this IAssignable<Func<T>> @this, T instance ) => @this.Configured( Factory.For( instance ) );
		public static ICommand Configured<T>( this IAssignable<Func<T>> @this, Func<T> factory ) => new AssignCommand<Func<T>>( @this ).Fixed( factory );
		//public static ICommand ConfigureGlobal<T>( this IAssignable<Func<object, T>> @this, T instance ) => new AssignCommand<Func<object, T>>( @this ).Fixed( Factory.For( instance ).Wrap() );
		// public static ICommand ToGlobalConfiguration<T>( this IAssignable<Func<object, T>> @this, ISource<T> value ) => new ApplySourceConfigurationCommand<T>( value, @this );
	}
}