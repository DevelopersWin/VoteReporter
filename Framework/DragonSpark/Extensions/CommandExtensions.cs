using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using ICommand = System.Windows.Input.ICommand;

namespace DragonSpark.Extensions
{
	public static class CommandExtensions
	{
		public static ICommand<T> Cast<T>( this ICommand @this, Func<T, object> projection ) => new ProjectedCommand<T>( @this, projection );

		public static IEnumerable<T> ExecuteMany<T>( this IEnumerable<T> @this, object parameter ) where T : ICommand => @this.Introduce( parameter, tuple => tuple.Item1.AsExecuted( tuple.Item2 ) ).WhereAssigned().ToArray();
		
		public static void Run<T>( this ICommand<T> @this ) => @this.Execute( default(T) );

		public static T AsExecuted<T>( this T @this, object parameter ) where T : ICommand
		{
			var result = @this.CanExecute( parameter ) ? @this : default(T);
			if ( result != null )
			{
				result.Execute( parameter );
			}
			return result;
		}

		public static T AsExecuted<T, U>( this T @this, U parameter ) where T : ICommand<U>
		{
			var result = @this.CanExecute( parameter ) ? @this : default(T);
			if ( result != null )
			{
				result.Execute( parameter );
			}
			return result;
		}

		public static Action<T> ToDelegate<T>( this ICommand<T> @this ) => DelegateCache<T>.Default.Get( @this );
		class DelegateCache<T> : Cache<ICommand<T>, Action<T>>
		{
			public static DelegateCache<T> Default { get; } = new DelegateCache<T>();

			DelegateCache() : base( command => command.Execute ) {}
		}

		public static Action<object> ToDelegate( this ICommand @this ) => DelegateCache.Default.Get( @this );
		class DelegateCache : Cache<ICommand, Action<object>>
		{
			public static DelegateCache Default { get; } = new DelegateCache();

			DelegateCache() : base( command => command.Execute ) {}
		}
	}
}
