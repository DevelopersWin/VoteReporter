using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Testing.Framework.Setup
{
	public sealed class MetadataCustomizationFactory<T> : ValidatedParameterizedSourceBase<MethodBase, ImmutableArray<T>> where T : class
	{
		public static MetadataCustomizationFactory<T> Instance { get; } = new MetadataCustomizationFactory<T>();
		MetadataCustomizationFactory() {}

		public override ImmutableArray<T> Get( MethodBase parameter )
		{
			var result = new object[] { parameter.DeclaringType.Assembly, parameter.DeclaringType, parameter }
				.SelectMany( o => HostedValueLocator<T>.Instance.Get( o ).AsEnumerable() )
				.Prioritize()
				.ToImmutableArray();
			return result;
		}
	}
}