using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.Unity;
using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Activation.IoC
{
	public class LifetimeManagerFactory<T> : LifetimeManagerFactory where T : LifetimeManager
	{
		public LifetimeManagerFactory( IUnityContainer container ) : base( container, AttributedLifetimeFactory.Instance.ToDelegate() ) {}

		class AttributedLifetimeFactory : IoC.AttributedLifetimeFactory
		{
			public new static AttributedLifetimeFactory Instance { get; } = new AttributedLifetimeFactory();

			public override Type Get( Type parameter ) => base.Get( parameter ) ?? typeof(T);
		}
	}

	class AttributedLifetimeFactory : ValidatedParameterizedSourceBase<Type, Type>
	{
		public static ICache<Type, Type> Instance { get; } = new AttributedLifetimeFactory().ToCache();
			
		public override Type Get( Type parameter ) => 
			parameter
				.GetTypeInfo()
				.GetCustomAttribute<LifetimeManagerAttribute>()
				.AsTo<LifetimeManagerAttribute, Type>( x => x.LifetimeManagerType );
	}

	[Persistent]
	public class LifetimeManagerFactory : ValidatedParameterizedSourceBase<Type, LifetimeManager>
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

		public override LifetimeManager Get( Type parameter )
		{
			var type = lifetimeTypeFactory( parameter );
			
			var result = type.With( lifetimeResolver );
			return result;
		}
	}
}