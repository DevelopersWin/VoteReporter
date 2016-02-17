using System;

namespace DragonSpark.Windows.Markup
{
	public abstract class MarkupTargetValueSetterBase : IMarkupTargetValueSetter
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();
		
		public object SetValue( object value )
		{
			if ( monitor.IsApplied )
			{
				throw new ObjectDisposedException( GetType().FullName );
			}

			return Apply( value );
		}

		protected abstract object Apply( object value );

		protected bool IsDisposed => monitor.IsApplied;

		public void Dispose() => monitor.Apply( () =>
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		} );

		protected virtual void Dispose( bool disposing )
		{}
	}
}