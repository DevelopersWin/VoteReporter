using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Type = System.Type;

namespace DragonSpark.Composition
{

	public sealed class ServiceProviderFactory : TransformerBase<IServiceProvider>
	{
		[Export( typeof(ITransformer<IServiceProvider>) )]
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Get( IServiceProvider parameter )
		{
			var context = CompositionHostFactory.Instance.Create();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), primary, parameter );
			return result;
		}
	}

	public class ExportProvider : IExportProvider
	{
		readonly CompositionContext context;
		public ExportProvider( CompositionContext context )
		{
			this.context = context;
		}

		public ImmutableArray<T> GetExports<T>( string name ) => context.GetExports<T>( name ).WhereAssigned().Prioritize().ToImmutableArray();
	}

	public sealed class ServiceLocator : ServiceLocatorImplBase
	{
		public ServiceLocator( CompositionContext host )
		{
			Host = host;
		}

		public CompositionContext Host { get; }

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => Host.GetExports( serviceType, null );

		protected override object DoGetInstance(Type serviceType, string key) => Host.TryGet<object>( serviceType, key );
	}
}
