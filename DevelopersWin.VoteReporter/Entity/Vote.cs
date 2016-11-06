using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DragonSpark.Windows.Legacy.Entity;

namespace DevelopersWin.VoteReporter.Entity
{
	public class Vote : VoteBase
	{
		[Required]
		public virtual VoteGroup Group { get; set; }

		[LocalStorage, NotMapped]
		public Uri Location
		{
			get { return LocationStorage.With( s => new Uri( s ) ); }
			set { LocationStorage = value.With( uri => uri.ToString() ); }
		}	string LocationStorage { get; set; }

		[Collection]
		public virtual ICollection<Record> Records { get; set; }

		public Record Latest => Records.OrderByDescending( record => record.Created ).First();
	}
}