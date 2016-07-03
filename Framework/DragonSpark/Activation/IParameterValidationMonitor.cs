using DragonSpark.Runtime.Properties;
using System;

namespace DragonSpark.Activation
{
	public interface IParameterValidationMonitor
	{
		bool IsValid( object parameter );

		bool IsWatching( object parameter );

		void MarkValid( object parameter, bool valid );
	}

	class ParameterValidationMonitor : IParameterValidationMonitor
	{
		readonly ICache<int, object> cache;

		public ParameterValidationMonitor() : this( new ArgumentCache<int, object>() ) {}

		public ParameterValidationMonitor( ICache<int, object> cache )
		{
			this.cache = cache;
		}

		public bool IsValid( object parameter ) => IsWatching( parameter ) && Equals( cache.Get( Environment.CurrentManagedThreadId ), parameter );
		public bool IsWatching( object parameter ) => cache.Contains( Environment.CurrentManagedThreadId );

		public void MarkValid( object parameter, bool valid )
		{
			if ( valid )
			{
				cache.Set( Environment.CurrentManagedThreadId, parameter );
			}
			else
			{
				cache.Remove( Environment.CurrentManagedThreadId );
			}
		}
	}

	public static class ParameterValidationMonitorExtensions
	{
		public static bool Update( this IParameterValidationMonitor @this, object parameter, Func<bool> source ) => @this.IsValid( parameter ) || @this.MarkAsValid( parameter, source() );

		public static bool MarkAsValid( this IParameterValidationMonitor @this, object parameter, bool valid )
		{
			@this.MarkValid( parameter, valid );
			return valid;
		}
	}
}