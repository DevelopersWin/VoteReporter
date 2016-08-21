using DragonSpark.ComponentModel;
using System.ComponentModel;
using Xunit;

namespace DragonSpark.Testing.Aspects
{
	public class BuildUpTests
	{
		[Fact]
		public void Build()
		{
			var sut = new BuildTarget();
			Assert.True( sut.Boolean );
			sut.Boolean = false;

			Assert.Null( sut.Value );
			sut.Call();
			Assert.False( sut.Boolean );
			Assert.Same( SkipFirstCallProvider.Default.Item, sut.Value );
		}

		public class BuildTarget
		{
			[DefaultValue( true )]
			public bool Boolean { get; set; }

			[SkipFirstCallValue]
			public object Value { get; set; }

			public void Call() => Value = SkipFirstCallProvider.Default.Item;
		}

		public class SkipFirstCallValue : DefaultValueBase
		{
			public SkipFirstCallValue() : base( t => SkipFirstCallProvider.Default )
			{}
		}

		public class SkipFirstCallProvider : IDefaultValueProvider
		{
			public static SkipFirstCallProvider Default { get; } = new SkipFirstCallProvider();

			readonly public object Item = new object();

			readonly ConditionMonitor monitor = new ConditionMonitor();

			public object GetValue( DefaultValueParameter parameter ) => monitor.Apply() ? null : Item;
		}
	}
}