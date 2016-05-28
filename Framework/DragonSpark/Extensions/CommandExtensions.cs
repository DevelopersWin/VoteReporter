using DragonSpark.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using ICommand = System.Windows.Input.ICommand;

namespace DragonSpark.Extensions
{
	public static class CommandExtensions
	{
		// public static ICommand<T> Cast<T>( this ICommand @this ) => Cast<T>( @this, Default<T>.Boxed );

		public static ICommand<T> Cast<T>( this ICommand @this, Func<T, object> box ) => new ProjectedCommand<T>( @this, box );

		public static IEnumerable<T> ExecuteMany<T>( this IEnumerable<T> @this, object parameter ) where T : ICommand => @this.Select( x => x.AsExecuted( parameter ) ).NotNull().ToArray();

		// public static void Run( this ICommand @this ) => @this.Execute( default(object) );

		public static void Run<T>( this ICommand<T> @this ) => @this.Run( default(T) );

		public static void Run<T>( this ICommand<T> @this, T parameter ) => @this.Execute( parameter );

		/*public static T Executed<T, TParameter>( this T @this, TParameter parameter ) where T : ICommand<TParameter> 
			=> Executed<T>( @this, parameter );*/

		public static T AsExecuted<T>( this T @this, object parameter ) where T : ICommand
		{
			var result = @this.CanExecute( parameter ) ? @this : default(T);
			result.With( x => x.Execute( parameter ) );
			return result;
		}

		public static T AsExecuted<T, U>( this T @this, U parameter ) where T : ICommand<U>
		{
			var result = @this.CanExecute( parameter ) ? @this : default(T);
			result.With( x => x.Execute( parameter ) );
			return result;
		}
	}
}
