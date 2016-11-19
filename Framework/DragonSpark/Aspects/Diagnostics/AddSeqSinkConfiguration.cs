using DragonSpark.Runtime.Data;
using JetBrains.Annotations;
using System;
using System.Runtime.Serialization;

namespace DragonSpark.Aspects.Diagnostics
{
	[DataContract]
	public sealed class AddSeqSinkConfiguration : DtoBase<DragonSpark.Diagnostics.Configurations.AddSeqSinkConfiguration>
	{
		[UsedImplicitly, DataMember]
		public Uri Endpoint { get; set; }

		[UsedImplicitly, DataMember]
		public int BatchPostingLimit { get; set; } = 1000;

		[UsedImplicitly, DataMember]
		public TimeSpan? Period { get; set; }

		[UsedImplicitly, DataMember]
		public string ApiKey { get; set; }

		[UsedImplicitly, DataMember]
		public string BufferBaseFileName { get; set; }

		[UsedImplicitly, DataMember]
		public long? BufferFileSizeLimitBytes { get; set; }
	}
}