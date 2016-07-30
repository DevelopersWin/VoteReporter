using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public sealed class MetadataCustomizationFactory<T> : FactoryBase<MethodBase, ImmutableArray<T>> where T : class
	{
		public static MetadataCustomizationFactory<T> Instance { get; } = new MetadataCustomizationFactory<T>();
		MetadataCustomizationFactory() {}

		public override ImmutableArray<T> Create( MethodBase parameter )
		{
			var result = new object[] { parameter.DeclaringType.Assembly, parameter.DeclaringType, parameter }
				.SelectMany( o => HostedValueLocator<T>.Instance.Create( o ).AsEnumerable() )
				.Prioritize()
				.ToImmutableArray();
			return result;
		}
	}
}