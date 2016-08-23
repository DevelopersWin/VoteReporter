using System;
using System.Composition;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Activation.Location
{
	public class SingletonLocator : FactoryCache<Type, object>, ISingletonLocator
	{
		[Export( typeof(ISingletonLocator) )]
		public static SingletonLocator Default { get; } = new SingletonLocator();
		SingletonLocator() : this( SingletonDelegates.Default.Get ) {}

		readonly Func<Type, Func<object>> provider;

		public SingletonLocator( Func<Type, Func<object>> provider )
		{
			this.provider = provider;
		}

		protected override object Create( Type parameter ) => provider( parameter )?.Invoke();
	}
}