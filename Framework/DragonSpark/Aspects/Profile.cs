using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Aspects
{
	public class Profile : ItemSource<IAspectInstance>, IProfile
	{
		protected Profile( Type declaringType, params IAspectInstance[] locators ) : base( locators )
		{
			DeclaringType = declaringType;
		}

		public Type DeclaringType { get; }
	}
}