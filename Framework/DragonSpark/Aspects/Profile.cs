using System;
using DragonSpark.Sources;

namespace DragonSpark.Aspects
{
	public class Profile : ItemSource<IAspectSource>, IProfile
	{
		protected Profile( Type declaringType, params IAspectSource[] sources ) : base( sources )
		{
			DeclaringType = declaringType;
		}

		public Type DeclaringType { get; }
	}
}