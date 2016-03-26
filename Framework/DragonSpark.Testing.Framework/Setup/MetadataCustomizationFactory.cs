using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using Ploeh.AutoFixture;
using System.Linq;
using System.Reflection;
using DragonSpark.Activation;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCustomizationFactory : FactoryBase<MethodBase, ICustomization[]>
	{
		public static MetadataCustomizationFactory Instance { get; } = new MetadataCustomizationFactory();

		protected override ICustomization[] CreateItem( MethodBase parameter )
		{
			var result = new object[] { parameter.DeclaringType.Assembly, parameter.DeclaringType, parameter }
				.SelectMany( HostedValueLocator<ICustomization>.Instance.Create )
				.Prioritize()
				.Fixed();
			return result;
		}
	}
}