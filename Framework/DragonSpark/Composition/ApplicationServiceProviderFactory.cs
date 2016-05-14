using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public class FactoryTypeFactory : FactoryBase<Type, FactoryTypeRequest>
	{
		public static FactoryTypeFactory Instance { get; } = new FactoryTypeFactory( Specification.Instance );

		public FactoryTypeFactory( ISpecification<Type> specification ) : base( specification ) {}

		public class Specification : CanInstantiateSpecification
		{
			public new static Specification Instance { get; } = new Specification();

			[Freeze]
			public override bool IsSatisfiedBy( Type parameter ) => base.IsSatisfiedBy( parameter ) && Factory.IsFactory( parameter ) && Factory.GetResultType( parameter ) != typeof(object) && parameter.Adapt().IsDefined<ExportAttribute>();
		}

		public override FactoryTypeRequest Create( Type parameter ) => new FactoryTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Factory.GetResultType( parameter ) );
	}

	public class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		static IServiceProvider Decorated { get; } = new DecoratedServiceProvider( Get );

		static object Get( Type type ) => DefaultServiceProvider.Instance.Value.GetService( type );

		readonly Func<CompositionContext> source;

		public ServiceProviderFactory( [Required] Type[] types ) : this( new Func<ContainerConfiguration>( new TypeBasedConfigurationContainerFactory( types ).Create ) ) {}

		public ServiceProviderFactory( [Required] Assembly[] assemblies ) : this( new Func<ContainerConfiguration>( new AssemblyBasedConfigurationContainerFactory( assemblies ).Create ) ) {}

		public ServiceProviderFactory( Func<ContainerConfiguration> configuration ) : this( new Func<CompositionContext>( new CompositionFactory( configuration ).Create ) ) {}

		public ServiceProviderFactory( [Required] Func<CompositionContext> source )
		{
			this.source = source;
		}

		protected override IServiceProvider CreateItem()
		{
			var context = source();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), new RecursionAwareServiceProvider( primary ), Decorated );
			return result;
		}
	}

	public class ServiceLocator : ServiceLocatorImplBase
	{
		readonly CompositionContext host;

		public ServiceLocator( [Required]CompositionContext host )
		{
			this.host = host;
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => host.GetExports( serviceType, null );

		protected override object DoGetInstance(Type serviceType, string key) => host.TryGet<object>( serviceType, key );
	}
}
