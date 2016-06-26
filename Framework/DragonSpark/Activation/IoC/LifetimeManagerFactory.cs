using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.Unity;
using System;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	/*public class ContainerControlledLifetimeManager : Microsoft.Practices.Unity.ContainerControlledLifetimeManager
	{
		protected override void Dispose( bool disposing )
		{
			var activated = GetValue().With( ActivationProperties.IsActivatedInstanceSpecification.Instance.IsSatisfiedBy );
			if ( !activated )
			{
				base.Dispose( disposing );
			}
		}
	}*/

	public class LifetimeManagerFactory<T> : LifetimeManagerFactory where T : LifetimeManager
	{
		public LifetimeManagerFactory( IUnityContainer container ) : base( container, AttributedLifetimeFactory.Instance.ToDelegate() ) {}

		class AttributedLifetimeFactory : IoC.AttributedLifetimeFactory
		{
			public new static AttributedLifetimeFactory Instance { get; } = new AttributedLifetimeFactory();

			public override Type Create( Type parameter ) => base.Create( parameter ) ?? typeof(T);
		}
	}

	class AttributedLifetimeFactory : FactoryBase<Type, Type>
	{
		public static AttributedLifetimeFactory Instance { get; } = new AttributedLifetimeFactory();
			
		[Freeze]
		public override Type Create( Type parameter ) => 
			parameter
				.GetTypeInfo()
				.GetCustomAttribute<LifetimeManagerAttribute>()
				.AsTo<LifetimeManagerAttribute, Type>( x => x.LifetimeManagerType );
	}

	[Persistent]
	public class LifetimeManagerFactory : FactoryBase<Type, LifetimeManager>
	{
		readonly Func<Type, LifetimeManager> lifetimeResolver;
		readonly Func<Type, Type> lifetimeTypeFactory;

		public LifetimeManagerFactory( IUnityContainer container ) : this( container, AttributedLifetimeFactory.Instance.ToDelegate() ) {}

		protected LifetimeManagerFactory( IUnityContainer container, Func<Type, Type> lifetimeTypeFactory ) : this( container.Resolve<LifetimeManager>, lifetimeTypeFactory ) {}

		LifetimeManagerFactory( Func<Type, LifetimeManager> lifetimeResolver, Func<Type, Type> lifetimeTypeFactory )
		{
			this.lifetimeResolver = lifetimeResolver;
			this.lifetimeTypeFactory = lifetimeTypeFactory;
		}

		public override LifetimeManager Create( Type parameter )
		{
			var type = lifetimeTypeFactory( parameter );
			
			var result = type.With( lifetimeResolver );
			return result;
		}
	}
}