using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelay : RelayMethodBase
	{
		public CommandRelay() : base( AdapterInvocation.Default ) {}

		sealed class AdapterInvocation : AdapterInvocation<ICommandAdapter>
		{
			public new static AdapterInvocation Default { get; } = new AdapterInvocation();
			AdapterInvocation() /*: base( SourceCoercer<ISpecificationRelayAdapter>.Default.To( CastCoercer<ISpecificationRelayAdapter, ICommandAdapter>.Default ).Get )*/ {}
			
			protected override object Apply( ICommandAdapter adapter, object parameter = null )
			{
				adapter.Execute( parameter );
				return null;
			}
		}
		
	}
}