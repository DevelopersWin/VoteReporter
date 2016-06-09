using System;

namespace DragonSpark.Extensions
{
	public static class ServiceProviderExtensions
	{
		public static TService Get<TService>( this IServiceProvider serviceProvider ) => Get<TService>( serviceProvider, typeof(TService) );

		public static TService Get<TService>( this IServiceProvider serviceProvider, Type type ) => (TService)serviceProvider.GetService( type );

		/*public static TService Get<TService>( this object @this )
		{
			var result = @this.AsTo<IServiceProvider, TService>( x => x.Get<TService>() );
			return result;
		}*/
	}
}