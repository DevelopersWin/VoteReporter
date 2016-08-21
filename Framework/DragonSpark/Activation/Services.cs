using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Activation
{
	public sealed class GlobalServiceProvider : Scope<IServiceProvider>
	{
		public static IScope<IServiceProvider> Default { get; } = new GlobalServiceProvider();

		GlobalServiceProvider() : base( () => DefaultServiceProvider.Default ) {}

		public static T GetService<T>() => GetService<T>( typeof(T) );

		public static T GetService<T>( Type type ) => Default.Get().Get<T>( type );
	}

	public sealed class DefaultServiceProvider : CompositeServiceProvider
	{
		public static IServiceProvider Default { get; } = new DefaultServiceProvider();
		DefaultServiceProvider() : base( new SourceServiceProvider( GlobalServiceProvider.Default, Activator.Default, Exports.Default, ApplicationParts.Default, ApplicationAssemblies.Default, ApplicationTypes.Default, LoggingHistory.Default, LoggingController.Default, Logger.Default.ToScope(), Instances.Default ), new DecoratedServiceProvider( Instances.Get<object> ), new DecoratedServiceProvider( Activator.Activate<object> ) ) {}
	}

	

	public class DecoratedServiceProvider : IServiceProvider
	{
		readonly Func<Type, object> inner;

		public DecoratedServiceProvider( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedServiceProvider( Func<Type, object> inner )
		{
			this.inner = inner;
		}

		public virtual object GetService( Type serviceType ) => inner( serviceType );
	}
}