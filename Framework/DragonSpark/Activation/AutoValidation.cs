using DragonSpark.Aspects;
using DragonSpark.Runtime;
using System;
using System.Collections.Concurrent;

namespace DragonSpark.Activation
{
	public interface IAutoValidationController 
	{
		bool IsValid( object parameter );

		object Execute( object parameter );

		void MarkValid( object parameter, bool valid );
	}

	public class AutoValidationController : IAutoValidationController
	{
		readonly ConcurrentDictionary<int, object> validated = new ConcurrentDictionary<int, object>();
		readonly IParameterValidationAdapter adapter;

		public AutoValidationController( IParameterValidationAdapter adapter )
		{
			this.adapter = adapter;
		}

		public bool IsValid( object parameter ) => CheckValid( parameter ) || AssignValid( parameter );

		bool AssignValid( object parameter )
		{
			var result = adapter.IsValid( parameter );
			MarkValid( parameter, result );
			return result;
		}

		bool CheckValid( object parameter )
		{
			object stored;
			return validated.TryGetValue( Environment.CurrentManagedThreadId, out stored ) && Equals( stored, parameter ?? Placeholders.Null );
		}

		public void MarkValid( object parameter, bool valid )
		{
			if ( valid )
			{
				validated[Environment.CurrentManagedThreadId] = parameter ?? Placeholders.Null;
			}
			else
			{
				object stored;
				validated.TryRemove( Environment.CurrentManagedThreadId, out stored );
			}
		}

		public object Execute( object parameter )
		{
			var result = IsValid( parameter ) ? adapter.Execute( parameter ) : null;
			MarkValid( parameter, false );
			return result;
		}
	}
}