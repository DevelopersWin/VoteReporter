using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;

namespace DragonSpark.Aspects.Validation
{
	sealed class AutoValidationController : ConcurrentDictionary<int, object>, IAutoValidationController, IAspectHub
	{
		// readonly static object Executing = new object();

		readonly IParameterValidationAdapter validator;
		
		public AutoValidationController( IParameterValidationAdapter validator )
		{
			this.validator = validator;
		}

		public bool IsSatisfiedBy( object parameter ) => Handler?.Handles( parameter ) ?? false;

		bool IsMarked( object parameter )
		{
			object current;
			var result = TryGetValue( Environment.CurrentManagedThreadId, out current ) && Equals( current, parameter ) && Clear();
			return result;
		}

		public void MarkValid( object parameter, bool valid )
		{
			if ( valid )
			{
				this[Environment.CurrentManagedThreadId] = parameter;
			}
			else
			{
				Clear();
			}
		}

		new bool Clear()
		{
			object removed;
			TryRemove( Environment.CurrentManagedThreadId, out removed );
			return true;
		}

		public object Execute( object parameter, Func<object> proceed )
		{
			object handled = null;
			if ( Handler?.Handle( parameter, out handled ) ?? false )
			{
				Clear();
				return handled;
			}

			var result = IsMarked( parameter ) || validator.IsSatisfiedBy( parameter ) ? proceed() : null;
			Clear();
			return result;
		}

		IParameterAwareHandler Handler { get; set; }
		
		public void Register( IAspect aspect )
		{
			var methodAware = aspect as IMethodAware;
			if ( methodAware != null && validator.IsSatisfiedBy( methodAware.Method ) )
			{
				var handler = aspect as IParameterAwareHandler;
				if ( handler != null )
				{
					Handler = Handler != null ? new LinkedParameterAwareHandler( handler, Handler ) : handler;
				}
			}
		}
	}
}