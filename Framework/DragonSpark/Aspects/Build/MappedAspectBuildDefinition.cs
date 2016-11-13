using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Build
{
	public class MappedAspectBuildDefinition : AspectBuildDefinition
	{
		public MappedAspectBuildDefinition( IDictionary<ITypeDefinition, IEnumerable<IAspects>> selectors ) : base( new AspectDefinitionSelector( selectors.TryGet ), selectors.Keys.Fixed() ) {}
		public MappedAspectBuildDefinition( IDictionary<ITypeDefinition, IAspects> selectors ) : base( new AspectDefinitionSelector( selectors.Yield ), selectors.Keys.Fixed() ) {}
	}
}