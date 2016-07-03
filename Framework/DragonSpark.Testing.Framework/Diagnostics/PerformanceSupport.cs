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

		readonly Action[] actions;

		public PerformanceSupport( params Action[] actions )
		{
			this.actions = actions;

			foreach ( var action in actions )
			{
				using ( action.Method.Profile( Ignore ) )
				{
					action();
				}
			}
		}

		static TimeSpan Average( Action action, int times = 100 )
		{
			var list = new List<TimeSpan>();
			for ( int j = 0; j < times; j++ )
			{
				using ( var profiler = action.Method.Profile( Ignore ) )
				{
					for ( int i = 0; i < 10000; i++ )
					{
						action();
					}
					profiler.Pause();
					list.Add( profiler.Elapsed );
				}
			}
			var result = TimeSpan.FromTicks( (long)list.Average( span => span.Ticks ) );
			return result;
		}

		public void Run( Action<string> output )
		{
			foreach ( var action in actions )
			{
				output( $"{Average( action )} {action.Method.Name}" );
			}
		}
	}
}
