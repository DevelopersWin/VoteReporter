using System.Reflection;
using DragonSpark.Activation;
using Microsoft.Practices.ServiceLocation;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;

namespace DragonSpark.Testing.Framework.Parameters
{
	public class FromCustomization : CustomizeAttribute
	{
		class Customization : ICustomization
		{
			public static Customization Instance { get; } = new Customization();

			public void Customize( IFixture fixture ) => fixture.Freeze( Services.Get<IServiceLocator>() );
		}

		public override ICustomization GetCustomization( ParameterInfo parameter ) => Customization.Instance;
	}
}