using DragonSpark.Activation;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Setup
{
	[Persistent]
	public class AllTypesOfFactory : FactoryBase<Type, Array>
	{
		readonly Assembly[] assemblies;
		readonly IActivator activator;

		public AllTypesOfFactory( [Required]Assembly[] assemblies, [Required]IActivator activator )
		{
			this.assemblies = assemblies;
			this.activator = activator;
		}

		public T[] Create<T>() => Create( typeof(T) ).Cast<T>().ToArray();

		protected override Array CreateItem( Type parameter )
		{
			var types = assemblies.SelectMany( assembly => assembly.ExportedTypes );
			var result = activator.ActivateMany( parameter, types ).ToArray();
			return result;
		}
	}
}