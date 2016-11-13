using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelay : RelayMethodBase
	{
		public CommandRelay() : base( SourceCoercer<ICommandAdapter>.Default.Get ) {}

		/*sealed class AdapterInvocation : AdapterInvocation<ICommandAdapter>
		{
			public new static AdapterInvocation Default { get; } = new AdapterInvocation();
			AdapterInvocation() /*: base( SourceCoercer<ISpecificationRelayAdapter>.Default.To( CastCoercer<ISpecificationRelayAdapter, ICommandAdapter>.Default ).Get )#1# {}
			
			protected override object Apply( ICommandAdapter adapter, object parameter = null )
			{
				adapter.Get( parameter );
				return null;
			}
		}*/
		
	}
}