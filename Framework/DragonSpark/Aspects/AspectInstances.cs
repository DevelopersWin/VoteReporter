using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Aspects
{
	public class AspectInstances : ItemSource<IAspectInstanceLocator>, IAspectInstances
	{
		protected AspectInstances( Type declaringType, params IAspectInstanceLocator[] locators ) : base( locators )
		{
			DeclaringType = declaringType;
		}

		public Type DeclaringType { get; }
	}
}