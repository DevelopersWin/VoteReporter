using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ISource<ICommandAdapter>) )]
	public sealed class ApplyCommandRelay : SpecificationRelayAspectBase, ISource<ICommandAdapter>
	{
		readonly ICommandAdapter adapter;

		public ApplyCommandRelay() : base( ApplyCommandRelayDefinition.Default ) {}

		[UsedImplicitly]
		public ApplyCommandRelay( ICommandAdapter adapter )
		{
			this.adapter = adapter;
		}

		public ICommandAdapter Get() => adapter;
		object ISource.Get() => Get();
	}
}