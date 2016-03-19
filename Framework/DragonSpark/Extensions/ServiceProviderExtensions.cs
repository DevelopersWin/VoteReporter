using System;

namespace DragonSpark.Extensions
{
	public static class ServiceProviderExtensions
	{
		public static TService Get<TService>( this IServiceProvider serviceProvider ) where TService : class => serviceProvider.GetService( typeof(TService) ).As<TService>();

		/*public static TService Get<TService>( this object @this )
		{
			var result = @this.AsTo<IServiceProvider, TService>( x => x.Get<TService>() );
			return result;
		}*/
	}
}