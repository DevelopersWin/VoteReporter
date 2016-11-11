using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ISource<IParameterizedSourceAdapter>) )]
	public sealed class ApplyParameterizedSourceRelay : InstanceAspectBase, ISource<IParameterizedSourceAdapter>
	{
		readonly IParameterizedSourceAdapter adapter;

		public ApplyParameterizedSourceRelay() : base( SourceRelaySelectors.Default.Get, SourceRelayDefinition.Default ) {}

		[UsedImplicitly]
		public ApplyParameterizedSourceRelay( IParameterizedSourceAdapter adapter )
		{
			this.adapter = adapter;
		}

		public IParameterizedSourceAdapter Get() => adapter;
		// object ISource.Get() => Get();
	}
}