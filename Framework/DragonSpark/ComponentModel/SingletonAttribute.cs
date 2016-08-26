using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.ComponentModel
{
	public class SingletonAttribute : DefaultValueBase
	{
		// public SingletonAttribute() : this( null ) {}

		public SingletonAttribute( Type hostType, string propertyName = nameof(SingletonLocator.Default) ) : base( new SingletonDefaultValueProvider( hostType, propertyName ).Wrap() ) {}
	}
}