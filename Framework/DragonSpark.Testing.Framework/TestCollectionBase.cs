using DragonSpark.Runtime;
using DragonSpark.Sources;
using JetBrains.dotMemoryUnit;
using System;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework
{
	public abstract class TestCollectionBase : DisposableBase
	{
		protected TestCollectionBase( ITestOutputHelper output )
		{
			WriteLine = output.WriteLine;
			Output.Default.Assign( WriteLine );
			DotMemoryUnitTestOutput.SetOutputMethod( WriteLine );
		}

		protected Action<string> WriteLine { get; }
	}
}