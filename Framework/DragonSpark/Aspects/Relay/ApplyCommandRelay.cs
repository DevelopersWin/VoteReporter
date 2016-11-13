using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ISource<ICommandAdapter>) )]
	public sealed class ApplyCommandRelay : ApplySpecificationRelayBase, ISource<ICommandAdapter>
	{
		readonly ICommandAdapter adapter;

		public ApplyCommandRelay() : base( CommandRelaySelectors.Default.Get, CommandRelayDefinition.Default ) {}

		[UsedImplicitly]
		public ApplyCommandRelay( ICommandAdapter adapter ) : base( adapter )
		{
			this.adapter = adapter;
		}

		ICommandAdapter ISource<ICommandAdapter>.Get() => adapter;
	}
}