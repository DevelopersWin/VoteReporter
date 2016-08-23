using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects
{
	public class OfSourceType : OfTypeAttribute
	{
		public OfSourceType() : base( typeof(IValidatedParameterizedSource), typeof(Func<>), typeof(Func<,>) ) {}
	}
}