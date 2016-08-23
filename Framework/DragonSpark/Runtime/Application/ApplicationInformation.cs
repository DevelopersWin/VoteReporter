using System;
using DragonSpark.ComponentModel;
using DragonSpark.TypeSystem;

namespace DragonSpark.Runtime.Application
{
	public class ApplicationInformation
	{
		[Service]
		public AssemblyInformation AssemblyInformation { get; set; }

		public Uri CompanyUri { get; set; }
		
		public DateTimeOffset? DeploymentDate { get; set; }
	}
}