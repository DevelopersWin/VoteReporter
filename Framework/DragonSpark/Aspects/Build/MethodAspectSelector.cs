using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	public sealed class MethodAspectSelector<T> : ParameterizedItemSourceBase<ITypeDefinition, IAspectDefinition>
		where T : IMethodLevelAspect
	{
		public static MethodAspectSelector<T> Default { get; } = new MethodAspectSelector<T>();
		MethodAspectSelector() {}

		public override IEnumerable<IAspectDefinition> Yield( ITypeDefinition parameter ) => 
			parameter.Select( store => new MethodAspectDefinition<T>( store ) );
	}
}