using DragonSpark.Activation;
using DragonSpark.Setup.Registration;
using System;
using System.Linq;
using Type = System.Type;

namespace DragonSpark.Setup
{
	[Persistent]
	public class AllTypesOfFactory : FactoryBase<Type, Array>
	{
		readonly Type[] types;
		readonly IActivator activator;

		public AllTypesOfFactory( Type[] types, IActivator activator )
		{
			this.types = types;
			this.activator = activator;
		}

		public T[] Create<T>() => Create( typeof(T) ).Cast<T>().ToArray();

		public override Array Create( Type parameter ) => activator.ActivateMany<object>( parameter, types );
	}
}