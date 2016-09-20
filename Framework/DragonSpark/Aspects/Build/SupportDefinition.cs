using PostSharp.Aspects;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	public class SupportDefinition<T> : SupportDefinitionBase where T : IAspect
	{
		public SupportDefinition( params IDefinition[] definitions ) : base( SpecificationFactory.Default.Get( definitions ), definitions.AsEnumerable().Concat().Select( methodStore => new MethodBasedAspectInstanceLocator<T>( methodStore ) ).ToArray() ) {}
	}
}