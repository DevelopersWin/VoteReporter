using System.Collections.Generic;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;

namespace DragonSpark.Aspects.Build
{
	public class PairedAspectBuildDefinition : AspectBuildDefinition
	{
		public PairedAspectBuildDefinition( IDictionary<ITypeDefinition, IEnumerable<IAspectDefinition>> selectors ) : base( new AspectDefinitionSelector( selectors.TryGet ), selectors.Keys.Fixed() ) {}
		public PairedAspectBuildDefinition( IDictionary<ITypeDefinition, IAspectDefinition> selectors ) : base( new AspectDefinitionSelector( selectors.Yield ), selectors.Keys.Fixed() ) {}
	}
}