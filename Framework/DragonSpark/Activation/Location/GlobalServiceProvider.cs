using System;
using DragonSpark.Extensions;
using DragonSpark.Sources;

namespace DragonSpark.Activation.Location
{
	public sealed class GlobalServiceProvider : Scope<IServiceProvider>
	{
		public static IScope<IServiceProvider> Default { get; } = new GlobalServiceProvider();

		GlobalServiceProvider() : base( () => DefaultServiceProvider.Default ) {}

		public static T GetService<T>() => GetService<T>( typeof(T) );

		public static T GetService<T>( Type type ) => Default.Get().Get<T>( type );
	}
}