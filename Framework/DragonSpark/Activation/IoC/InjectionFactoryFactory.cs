using DragonSpark.Extensions;
using Microsoft.Practices.Unity;
using System;
using System.Linq;

namespace DragonSpark.Activation.IoC
{
	public class InjectionFactoryFactory : InjectionMemberFactory<InjectionFactory>
	{
		readonly Type factoryType;

		public InjectionFactoryFactory( Type factoryType )
		{
			this.factoryType = factoryType;
		}

		public override InjectionFactory Create( InjectionMemberParameter parameter )
		{
			var previous = parameter.Container.Registrations.FirstOrDefault( x => x.RegisteredType == parameter.TargetType && x.MappedToType != x.RegisteredType ).With( x => x.MappedToType );

			var result = new InjectionFactory( ( unityContainer, type, buildName ) =>
			{
				var item = unityContainer.Resolve<InstanceFromSourceTypeFactory>().Create( factoryType ) ?? previous.With( x => unityContainer.Resolve( x ) );
				return item;
			} );
			return result;
		}
	}
}