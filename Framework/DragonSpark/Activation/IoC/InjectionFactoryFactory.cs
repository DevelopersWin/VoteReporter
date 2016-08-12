using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
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

		public override InjectionFactory Get( InjectionMemberParameter parameter )
		{
			var previous = parameter.Container.Registrations.Introduce( parameter, x => x.Item1.RegisteredType == x.Item2.TargetType && x.Item1.MappedToType != x.Item1.RegisteredType ).FirstOrDefault()?.MappedToType;
			var result = new InjectionFactory( new Context( factoryType, previous ).Create );
			return result;
		}

		class Context
		{
			readonly Type factoryType;
			readonly Type previous;
			public Context( Type factoryType, Type previous )
			{
				this.factoryType = factoryType;
				this.previous = previous;
			}

			public object Create( IUnityContainer unityContainer, Type type, string buildName ) => SourceFactory.Default.Get( factoryType ) ?? ( previous != null ? unityContainer.Resolve( previous, Items<ResolverOverride>.Default ) : null );
		}
	}
}