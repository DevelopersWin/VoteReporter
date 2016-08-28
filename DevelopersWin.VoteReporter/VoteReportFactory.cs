using DevelopersWin.VoteReporter.Entity;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace DevelopersWin.VoteReporter
{
	[Export]
	public class VoteReportFactory : SourceBase<VoteReport>
	{
		readonly VotingContext context;

		public VoteReportFactory( VotingContext context )
		{
			this.context = context;
		}

		public override VoteReport Get()
		{
			var recordings = context.Recordings.OrderByDescending( recording => recording.Created );
			var current = recordings.First();
		    var firstOrDefault = recordings.FirstOrDefault( recording => recording.Created < current.Created );
		    var previous = firstOrDefault.With( recording => Convert( recording, null ) );
			var result = Convert( current, previous );
			return result;
		}

		static VoteReport Convert( Recording source, VoteReport reference )
		{
			var result = source.MapInto<VoteReport>().With<VoteReport>( report => report.Groups.AddRange( DetermineGroups( source, reference ) ) );
			return result;
		}

		static IEnumerable<VoteGroupView> DetermineGroups( Recording current, VoteReport previous )
		{
			var groups = current.Records.GroupBy( record => record.Vote.Group ).OrderBy( grouping => grouping.Key.Order ).Select( records => records.Key );
			var result = groups.Select( @group => Create( current, @group, previous.With( x => x.Groups.SingleOrDefault( y => y.Id == @group.Id ) ) ) ).ToArray();
			return result;
		}

		static VoteGroupView Create( Recording recording, VoteGroup current, VoteGroupView previous )
		{
			var result = current.MapInto<VoteGroupView>();
			var count = current.Votes.Sum( vote => vote.Records.SingleOrDefault( record => record.Recording == recording ).With( record => record.Count ) );
			result.Counts = new VoteCount { Count = count, Delta = count - previous.With( view => view.Counts.Count ) };
			result.Votes.AddRange( 
				current.Votes
					.OrderBy( vote => vote.Order )
					.Select( v => CreateVote( recording, v, previous.With( x => x.Votes.SingleOrDefault( y => y.Id == v.Id ) ) ) )
					.OrderByDescending( view => view.Counts.Delta )
				);
			return result;
		}

		static VoteView CreateVote( Recording recording, Vote current, VoteView previous )
		{
			var result = current.MapInto<VoteView>();
			var count = current.Records.SingleOrDefault( record => record.Recording == recording ).With( record => record.Count );
			result.Counts = new VoteCount { Count = count, Delta = count - previous.With( view => view.Counts.Count ) };
			return result;
		}
	}

	public class VoteReport : ViewBase
	{
		public System.Collections.ObjectModel.Collection<VoteGroupView> Groups { get; } = new System.Collections.ObjectModel.Collection<VoteGroupView>();
	}

	public abstract class VoteViewBase : ViewBase
	{
		public string Title { get; set; }

		public VoteCount Counts { get; set; }
	}

	public class VoteCount : ViewBase
	{
		public int Count { get; set; }

		public int Delta { get; set; }
	}

	public class VoteGroupView : VoteViewBase
	{
		public System.Collections.ObjectModel.Collection<VoteView> Votes { get; } = new System.Collections.ObjectModel.Collection<VoteView>();
	}

	public class VoteView : VoteViewBase
	{
		public Uri Location { get; set; }
	}
}