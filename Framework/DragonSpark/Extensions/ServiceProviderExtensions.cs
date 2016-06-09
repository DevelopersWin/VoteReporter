using System;

namespace DragonSpark.Extensions
{
	public static class ServiceProviderExtensions
	{
		public static T Get<T>( this IServiceProvider serviceProvider ) => Get<T>( serviceProvider, typeof(T) );

		public static T Get<T>( this IServiceProvider serviceProvider, Type type ) => (T)serviceProvider.GetService( type );

		/*public static TService Get<TService>( this object @this )
		{
			var result = @this.AsTo<IServiceProvider, TService>( x => x.Get<TService>() );
			return result;
		}*/
	}
}