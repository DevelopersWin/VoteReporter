using DragonSpark.ComponentModel;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime
{
	public class ApplicationInformation
	{
		[Service]
		public AssemblyInformation AssemblyInformation { get; set; }

		public Uri CompanyUri { get; set; }
		
		public DateTimeOffset? DeploymentDate { get; set; }
	}
}