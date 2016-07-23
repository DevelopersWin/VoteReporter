using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using Ploeh.AutoFixture;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCustomizationFactory : FactoryBase<MethodBase, ImmutableArray<ICustomization>>
	{
		public static MetadataCustomizationFactory Instance { get; } = new MetadataCustomizationFactory();

		public override ImmutableArray<ICustomization> Create( MethodBase parameter )
		{
			var result = new object[] { parameter.DeclaringType.Assembly, parameter.DeclaringType, parameter }
				.SelectMany( HostedValueLocator<ICustomization>.Instance.Create )
				.Prioritize()
				.ToImmutableArray();
			return result;
		}
	}
}