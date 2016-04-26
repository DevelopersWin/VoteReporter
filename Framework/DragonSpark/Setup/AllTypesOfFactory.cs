using DragonSpark.Activation;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
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

		public AllTypesOfFactory( [Required]Type[] types, [Required]IActivator activator )
		{
			this.types = types;
			this.activator = activator;
		}

		public T[] Create<T>() => Create( typeof(T) ).Cast<T>().ToArray();

		protected override Array CreateItem( Type parameter )
		{
			var result = activator.ActivateMany( parameter, types ).ToArray();
			return result;
		}
	}
}