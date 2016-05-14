using DragonSpark.Activation;
using DragonSpark.Testing.Objects;

namespace DragonSpark.Windows.Testing.TestObjects
{
	public class Locator : LocatorBase
	{
		public override object Create( LocateTypeRequest parameter ) => 
			parameter.RequestedType == typeof(Object) ? new Object { Name = parameter.Name ?? "DefaultActivation" } : null;
	}

	public class Constructor : ConstructorBase
	{
		public override object Create( ConstructTypeRequest parameter ) => parameter.RequestedType == typeof(Item) ? new Item { Parameters = parameter.Arguments } : null;
	}
}