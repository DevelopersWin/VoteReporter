using System.Linq;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public class SupportDefinition<T> : SupportDefinitionBase where T : IAspect
	{
		public SupportDefinition( params IDefinition[] definitions ) : base( SpecificationFactory.Default.Get( definitions ), definitions.AsEnumerable().Concat().Select( definition => new AspectInstanceLocator<T>( definition ) ).ToArray() ) {}
	}
}