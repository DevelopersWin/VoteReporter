using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;
using DragonSpark.Aspects;

namespace DragonSpark.Activation.IoC
{
	public class LifetimeManagerFactory<T> : LifetimeManagerFactory where T : LifetimeManager
	{
		public LifetimeManagerFactory( IUnityContainer container ) : base( container, AttributedLifetimeFactory.Instance.Create ) {}

		class AttributedLifetimeFactory : IoC.AttributedLifetimeFactory
		{
			public new static AttributedLifetimeFactory Instance { get; } = new AttributedLifetimeFactory();

			protected override Type CreateItem( Type parameter ) => base.CreateItem( parameter ) ?? typeof(T);
		}
	}

	class AttributedLifetimeFactory : FactoryBase<Type, Type>
	{
		public static AttributedLifetimeFactory Instance { get; } = new AttributedLifetimeFactory();
			
		[Freeze]
		protected override Type CreateItem( Type parameter ) => 
			parameter
				.GetTypeInfo()
				.GetCustomAttribute<LifetimeManagerAttribute>()
				.AsTo<LifetimeManagerAttribute, Type>( x => x.LifetimeManagerType );
	}

	[Persistent]
	public class LifetimeManagerFactory : FactoryBase<Type, LifetimeManager>
	{
		readonly IUnityContainer container;
		readonly Func<Type, Type> lifetimeTypeFactory;

		public LifetimeManagerFactory( [Required]IUnityContainer container ) : this( container, AttributedLifetimeFactory.Instance.Create ) {}

		protected LifetimeManagerFactory( [Required]IUnityContainer container, Func<Type, Type> lifetimeTypeFactory )
		{
			this.container = container;
			this.lifetimeTypeFactory = lifetimeTypeFactory;
		}

		protected override LifetimeManager CreateItem( Type parameter )
		{
			var type = lifetimeTypeFactory( parameter );
			var result = type.With( container.Resolve<LifetimeManager> );
			return result;
		}
	}
}