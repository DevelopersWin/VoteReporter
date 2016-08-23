using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public static class Defaults
	{
		public const int Retries = 5;

		public static Func<int, TimeSpan> Time { get; } = LinearRetryTime.Default.ToSourceDelegate();
	}
}