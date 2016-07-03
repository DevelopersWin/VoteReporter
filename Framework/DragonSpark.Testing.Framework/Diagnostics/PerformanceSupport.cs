using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public class PerformanceSupport
	{
		readonly static Action<string> Ignore = IgnoredOutputCommand.Instance.ToDelegate();

		readonly Action<string> output;
		readonly Action[] actions;

		public PerformanceSupport( Action<string> output, params Action[] actions )
		{
			this.output = output;
			this.actions = actions;

			foreach ( var action in actions )
			{
				using ( action.Method.Profile( Ignore ) )
				{
					action();
				}
			}
		}

		static TimeSpan Average( Action action, int numberOfRuns = 100, int perRun = 10000 )
		{
			var list = new List<TimeSpan>();
			for ( var j = 0; j < numberOfRuns; j++ )
			{
				using ( var profiler = action.Method.Profile( Ignore ) )
				{
					for ( var i = 0; i < perRun; i++ )
					{
						action();
					}
					profiler.Pause();
					list.Add( profiler.Elapsed );
				}
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
			var result = TimeSpan.FromTicks( (long)list.Average( span => span.Ticks ) );
			return result;
		}

		public void Run( int numberOfRuns = 100, int perRun = 10000 )
		{
			foreach ( var action in actions )
			{
				output( $"{Average( action, numberOfRuns, perRun ).ToString()} {action.Method.Name}" );
			}
		}
	}
}
