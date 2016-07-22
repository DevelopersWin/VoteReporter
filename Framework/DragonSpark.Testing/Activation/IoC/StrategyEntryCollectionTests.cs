using DragonSpark.Activation.IoC;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Activation.IoC
{
	public class StrategyEntryCollectionTests
	{
		readonly static StrategyEntry[] DefaultEntries = {
															new StrategyEntry( new HierarchicalLifetimeStrategy(), UnityBuildStage.Lifetime ),
															new StrategyEntry( new BuildPlanStrategy(), UnityBuildStage.Creation ),
															new StrategyEntry( new LifetimeStrategy(), UnityBuildStage.Lifetime, Priority.High ),
															new StrategyEntry( new BuildKeyMappingStrategy(), UnityBuildStage.TypeMapping ),
														 };

		[Fact]
		public void Basic()
		{
			var sut = new StrategyEntryCollection( DefaultEntries );
			var instances = sut.Instances();
			Assert.Empty( sut );
			Assert.Equal( new [] { DefaultEntries.Last(), DefaultEntries[2], DefaultEntries.First(), DefaultEntries[1] }.Select( entry => entry.Value ).ToArray(), instances.ToArray() );
		}
	}
}