using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.Unity;
using System;
using System.Reflection;
using DragonSpark.Sources.Caching;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Activation.IoC
{
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
		public static ICache<Type, Type> Instance { get; } = new AttributedLifetimeFactory().ToCache();
			
		public override Type Create( Type parameter ) => 
			parameter
				.GetTypeInfo()
				.GetCustomAttribute<LifetimeManagerAttribute>()
				.AsTo<LifetimeManagerAttribute, Type>( x => x.LifetimeManagerType );
	}

	[Persistent]
	public class LifetimeManagerFactory : FactoryBase<Type, LifetimeManager>
	{
		readonly static Func<Type, Type> LifetimeTypeFactory = AttributedLifetimeFactory.Instance.Get;

		readonly Func<Type, LifetimeManager> lifetimeResolver;
		readonly Func<Type, Type> lifetimeTypeFactory;

		public LifetimeManagerFactory( IUnityContainer container ) : this( container, LifetimeTypeFactory ) {}

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