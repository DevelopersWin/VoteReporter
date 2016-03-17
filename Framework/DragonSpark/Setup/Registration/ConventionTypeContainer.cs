using System;

namespace DragonSpark.Setup.Registration
{
	public class ConventionTypeContainer
	{
		public ConventionTypeContainer( Type[] candidates )
		{
			Candidates = candidates;
		}

		public Type[] Candidates { get; }
	}
}