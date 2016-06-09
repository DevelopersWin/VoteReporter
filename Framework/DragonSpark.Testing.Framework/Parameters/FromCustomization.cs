using DragonSpark.Activation;
using DragonSpark.Extensions;
using Microsoft.Practices.ServiceLocation;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Parameters
{
	public class FromCustomization : CustomizeAttribute
	{
		class Customization : ICustomization
		{
			public static Customization Instance { get; } = new Customization();

			public void Customize( IFixture fixture ) => fixture.Freeze( GlobalServiceProvider.Instance.Get<IServiceLocator>() );
		}

		public override ICustomization GetCustomization( ParameterInfo parameter ) => Customization.Instance;
	}
}