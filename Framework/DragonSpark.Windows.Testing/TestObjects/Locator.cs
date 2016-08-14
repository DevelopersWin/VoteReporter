using DragonSpark.Activation;
using DragonSpark.Testing.Objects;
using System.Composition;

namespace DragonSpark.Windows.Testing.TestObjects
{
	[Export( typeof(IActivator) )]
	public class Locator : LocatorBase
	{
		public override object Get( LocateTypeRequest parameter ) => 
			parameter.RequestedType == typeof(Object) ? new Object { Name = parameter.Name ?? "DefaultActivation" } : null;
	}

	public class Constructor : ConstructorBase
	{
		public override object Get( ConstructTypeRequest parameter ) => parameter.RequestedType == typeof(Item) ? new Item { Parameters = parameter.Arguments } : null;
	}
}