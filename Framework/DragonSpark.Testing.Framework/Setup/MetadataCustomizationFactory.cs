using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using Ploeh.AutoFixture;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCustomizationFactory : FactoryWithSpecificationBase<MethodBase, ICustomization[]>
	{
		public static MetadataCustomizationFactory Instance { get; } = new MetadataCustomizationFactory();

		public override ICustomization[] Create( MethodBase parameter )
		{
			var result = new object[] { parameter.DeclaringType.Assembly, parameter.DeclaringType, parameter }
				.SelectMany( HostedValueLocator<ICustomization>.Instance.Create )
				.Prioritize()
				.Fixed();
			return result;
		}
	}
}