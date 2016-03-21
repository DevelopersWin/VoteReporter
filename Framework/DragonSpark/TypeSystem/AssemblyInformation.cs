using System;
using System.Diagnostics;

namespace DragonSpark.TypeSystem
{
	public class AssemblyInformation
	{
		public AssemblyInformation()
		{
			Debugger.Break();
		}

		public string Title { get; set; }

		public string Product { get; set; }

		public string Company { get; set; }

		public string Description { get; set; }

		public string Configuration { get; set; }

		public string Copyright { get; set; }

		public Version Version { get; set; }
	}
}