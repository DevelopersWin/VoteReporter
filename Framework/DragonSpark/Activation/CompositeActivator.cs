using System.Linq;

namespace DragonSpark.Activation
{
	public class CompositeActivator : FirstFromParameterFactory<TypeRequest, object>, IActivator
	{
		public CompositeActivator( params IActivator[] locators ) 
			: base( locators.Select( activator => activator.ToDelegate() ).ToArray() ) {}
	}
}