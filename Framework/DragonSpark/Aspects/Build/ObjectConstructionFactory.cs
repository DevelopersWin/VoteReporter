using System.Collections.Generic;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Reflection;

namespace DragonSpark.Aspects.Build
{
	public sealed class ObjectConstructionFactory<T> : ParameterizedSourceBase<IEnumerable<object>, ObjectConstruction>
	{
		public static ObjectConstructionFactory<T> Default { get; } = new ObjectConstructionFactory<T>();
		ObjectConstructionFactory() {}

		public override ObjectConstruction Get( IEnumerable<object> parameter ) => new ObjectConstruction( typeof(T), parameter.Fixed() );
	}
}