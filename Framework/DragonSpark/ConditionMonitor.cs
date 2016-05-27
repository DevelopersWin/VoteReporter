using System;

namespace DragonSpark
{
	public class ConditionMonitor
	{
		public bool IsApplied => State > ConditionMonitorState.None;

		public ConditionMonitorState State { get; private set; }

		public void Reset() => State = ConditionMonitorState.None;

		public bool Apply() => ApplyIf( null, null );

		public bool Apply( Action action ) => ApplyIf( null, action );

		public bool ApplyIf( bool? condition ) => ApplyIf( condition, null );

		public bool ApplyIf( bool? condition, Action action )
		{
			switch ( State )
			{
				case ConditionMonitorState.None:
					State = ConditionMonitorState.Applying;
					var updated = condition.GetValueOrDefault( true );
					if ( updated )
					{
						action?.Invoke();
					}
					State = updated ? ConditionMonitorState.Applied : ConditionMonitorState.None;
					return updated;
			}
			return false;
		}
	}
}