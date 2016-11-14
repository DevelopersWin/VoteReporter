using DragonSpark.Activation.Location;
using DragonSpark.Sources;
using System;

namespace DragonSpark.ComponentModel
{
	public sealed class SingletonAttribute : DefaultValueBase
	{
		public SingletonAttribute( Type hostType = null, string propertyName = nameof(SingletonLocator.Default) ) : base( new SingletonDefaultValueProvider( hostType, propertyName ).Accept ) {}
	}
}