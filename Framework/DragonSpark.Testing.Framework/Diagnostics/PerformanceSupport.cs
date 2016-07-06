﻿using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public class PerformanceSupport
	{
		readonly static Action<string> Ignore = IgnoredOutputCommand.Instance.ToDelegate();
		readonly static string[] Titles = { "Test", "Average", "Median", "Mode" };
		const string TimeFormat = "ss'.'ffff";

		readonly Action<string> output;
		readonly ImmutableArray<Action> actions;

		public PerformanceSupport( Action<string> output, params Action[] actions )
		{
			this.output = output;
			this.actions = actions.ToImmutableArray();

			foreach ( var action in actions )
			{
				using ( action.Method.Profile( Ignore ) )
				{
					action();
				}
			}
		}

		class ReportFactory : FactoryBase<ReportFactory.Parameter, ImmutableArray<ReportFactory.Result>>
		{
			public static ReportFactory Instance { get; } = new ReportFactory();

			public struct Parameter
			{
				public Parameter( ImmutableArray<Action> actions, int numberOfRuns = 100, int perRun = 10000 )
				{
					Actions = actions;
					NumberOfRuns = numberOfRuns;
					PerRun = perRun;
				}

				public ImmutableArray<Action> Actions { get; }
				public int NumberOfRuns { get; }
				public int PerRun { get; }
			}
			
			public struct Result
			{
				public Result( string name, TimeSpan average, TimeSpan median, TimeSpan mode )
				{
					Name = name;
					Average = average;
					Median = median;
					Mode = mode;
				}

				public string Name { get; }
				public TimeSpan Average { get; }
				public TimeSpan Median { get; }
				public TimeSpan Mode { get; }
			}

			class MedianFactory : FactoryBase<ImmutableArray<long>, long>
			{
				public static MedianFactory Instance { get; } = new MedianFactory();

				public override long Create( ImmutableArray<long> parameter )
				{
					var length = parameter.Length;
					var middle = length / 2;
					var ordered = parameter.ToArray().OrderBy( i => i ).ToArray();
					var median = ordered.ElementAt( middle ) + ordered.ElementAt( ( length - 1 ) / 2 );
					var result = median / 2;
					return result;
				}
			}

			class ModeFactory<T> : FactoryBase<ImmutableArray<T>, T>
			{
				public static ModeFactory<T> Instance { get; } = new ModeFactory<T>();
				public override T Create( ImmutableArray<T> parameter ) => parameter.ToArray().GroupBy( n => n ).OrderByDescending( g => g.Count() ).Select( g => g.Key ).FirstOrDefault();
			}

			public override ImmutableArray<Result> Create( Parameter parameter ) => 
				parameter.Actions.Introduce( parameter, tuple => new Run( tuple.Item1, tuple.Item2.NumberOfRuns, tuple.Item2.PerRun ).Create() ).ToImmutableArray();

			class Run : FactoryBase<Result>
			{
				readonly Action action;
				readonly int numberOfRuns;
				readonly int perRun;
				
				public Run( Action action, int numberOfRuns, int perRun )
				{
					this.action = action;
					this.numberOfRuns = numberOfRuns;
					this.perRun = perRun;
				}

				public override Result Create()
				{
					var data = EnumerableEx.Generate( 0, Continue, i => i + 1, Measure ).Select( span => span.Ticks ).ToArray();
					var average = data.Average( span => span );
					var median = MedianFactory.Instance.Create( data.ToImmutableArray() );
					var mode = ModeFactory<long>.Instance.Create( data.ToImmutableArray() );
					var result = new Result( action.Method.Name, TimeSpan.FromTicks( (long)average ), TimeSpan.FromTicks( median ), TimeSpan.FromTicks( mode ) );
					return result;
				}

				bool Continue( int i ) => i < numberOfRuns;

				TimeSpan Measure<T>( T _ )
				{
					var timer = new Timer();
					using ( timer )
					{
						timer.Start();
						for ( var i = 0; i < perRun; i++ )
						{
							action();
						}
					}
					var result = timer.Elapsed;
					return result;
				}
			}
		}

		public void Run( int numberOfRuns = 100, int perRun = 10000 )
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			var results = ReportFactory.Instance.Create( new ReportFactory.Parameter( actions, numberOfRuns, perRun ) ).ToArray();

			var max = results.Max( r => r.Name.Length );
			var template = $"{{0,-{max}}} | {{1, 7}} | {{2, 7}} | {{3, 7}}";

			var title = string.Format( template, Titles );
			output( title );
			output( new string( '-', title.Length ) );

			foreach ( var result in results )
			{
				output( string.Format( template, result.Name, result.Average.ToString( TimeFormat ), result.Median.ToString( TimeFormat ), result.Mode.ToString( TimeFormat ) ) );
			}
		}
	}
}
