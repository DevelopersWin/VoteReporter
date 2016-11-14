using DragonSpark.Sources;
using System;

namespace DragonSpark.ComponentModel
{
	public sealed class CurrentTimeAttribute : DefaultValueBase
	{
		readonly static Func<object, CurrentTimeValueProvider> Provider = CurrentTimeValueProvider.Default.Accept;

		public CurrentTimeAttribute() : base( Provider ) {}
	}
}