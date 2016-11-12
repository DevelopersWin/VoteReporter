using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class DefaultParameterizedSourceImplementation : DelegatedParameterizedSource<object, object>
	{
		public DefaultParameterizedSourceImplementation( IParameterizedSourceAdapter adapter ) : base( adapter.Get ) {}
	}
}