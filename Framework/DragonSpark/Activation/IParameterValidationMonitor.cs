using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Activation
{
	public interface IParameterValidationMonitor
	{
		bool IsValid( object parameter );

		void MarkValid( object parameter, bool valid );

		void Clear( object parameter );
	}

	class ParameterValidationMonitor : IParameterValidationMonitor
	{
		readonly ICache<int, object> cache;

		public ParameterValidationMonitor() : this( new ArgumentCache<int, object>() ) {}

		public ParameterValidationMonitor( ICache<int, object> cache )
		{
			this.cache = cache;
		}

		public bool IsValid( object parameter ) => Equals( cache.Get( Environment.CurrentManagedThreadId ), parameter ?? SpecialValues.Null );

		public void MarkValid( object parameter, bool valid ) => cache.Set( Environment.CurrentManagedThreadId, valid ? ( parameter ?? SpecialValues.Null ) : null );

		public void Clear( object parameter ) => cache.Remove( Environment.CurrentManagedThreadId );
	}

	public static class ParameterValidationMonitorExtensions
	{
		public static bool Update( this IParameterValidationMonitor @this, object parameter, Func<bool> source ) => @this.IsValid( parameter ) || @this.MarkAsValid( parameter, source() );

		public static bool Update( this IParameterValidationMonitor @this, object parameter, Func<object, bool> source ) => @this.IsValid( parameter ) || @this.MarkAsValid( parameter, source( parameter ) );

		public static bool MarkAsValid( this IParameterValidationMonitor @this, object parameter, bool valid )
		{
			@this.MarkValid( parameter, valid );
			return valid;
		}
	}
}