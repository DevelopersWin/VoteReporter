using System;

namespace DragonSpark.Diagnostics.Logging
{
	public delegate void LogException<in T>( Exception exception, string template, T parameter );
}