using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Threading;
using DragonSpark.Aspects.Extensions;

namespace DragonSpark.Aspects.Validation
{
	sealed class AutoValidationController : ThreadLocal<object>, IAutoValidationController, IAspectHub
	{
		readonly IParameterValidationAdapter validator;
		
		public AutoValidationController( IParameterValidationAdapter validator )
		{
			this.validator = validator;
		}

		public bool IsSatisfiedBy( object parameter ) => Handler?.Handles( parameter ) ?? false;

		bool IsMarked( object parameter ) => Value != null && Equals( Value, parameter ) && Clear();

		public void MarkValid( object parameter, bool valid ) => Value = valid ? parameter : null;

		bool Clear()
		{
			Value = null;
			return true;
		}

		public object Execute( object parameter, IInvocation proceed )
		{
			object handled = null;
			if ( Handler?.Handle( parameter, out handled ) ?? false )
			{
				Clear();
				return handled;
			}

			var result = IsMarked( parameter ) || validator.IsSatisfiedBy( parameter ) ? proceed.Invoke( parameter ) : null;
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