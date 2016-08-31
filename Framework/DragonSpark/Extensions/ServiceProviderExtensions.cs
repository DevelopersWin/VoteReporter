using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Extensions
{
	public static class ServiceProviderExtensions
	{
		public static IServiceProvider Cached( this IServiceProvider @this ) => new DecoratedServiceProvider( new Cache<Type, object>( @this.GetService ).GetAssigned );

		public static T Get<T>( this IServiceProvider serviceProvider ) => Get<T>( serviceProvider, typeof(T) );

		public static T Get<T>( this IServiceProvider serviceProvider, Type type ) => (T)serviceProvider.GetService( type );

		public static Func<Type, T> Delegate<T>( this Func<IServiceProvider> @this ) => Delegates<T>.Default.Get( @this );
		sealed class Delegates<T> : Cache<Func<IServiceProvider>, Func<Type, T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() : base( source => new Factory( source ).Get ) {}

			sealed class Factory : ValidatedParameterizedSourceBase<Type, T>
			{
				readonly Func<IServiceProvider> source;
				public Factory( Func<IServiceProvider> source )
				{
					this.source = source;
				}

				public override T Get( Type parameter ) => source().Get<T>( parameter );

				public override bool IsSatisfiedBy( Type parameter ) => source().Accepts( parameter );
			}
		}
	}
}