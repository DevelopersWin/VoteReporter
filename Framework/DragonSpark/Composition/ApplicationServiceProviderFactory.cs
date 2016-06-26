﻿using DragonSpark.Activation;
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
			public override bool IsSatisfiedBy( Type parameter ) => base.IsSatisfiedBy( parameter ) && IsFactorySpecification.Instance.IsSatisfiedBy( parameter ) && ResultTypeLocator.Instance.Get( parameter ) != typeof(object) && parameter.Has<ExportAttribute>();
		}

		public override FactoryTypeRequest Create( Type parameter )
		{
			var resultType = ResultTypeLocator.Instance.Get( parameter );
			var request = new FactoryTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), resultType );
			return request;
		}
	}

	public class TypeBasedServiceProviderFactory : ServiceProviderFactory
	{
		public TypeBasedServiceProviderFactory( Type[] types ) : base( new TypeBasedConfigurationContainerFactory( types ) ) {}
	}

	public class AssemblyBasedServiceProviderFactory : ServiceProviderFactory
	{
		public AssemblyBasedServiceProviderFactory( Assembly[] assemblies ) : base( new AssemblyBasedConfigurationContainerFactory( assemblies ) ) {}
	}

	public abstract class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly IFactory<ContainerConfiguration> configuration;

		protected ServiceProviderFactory( IFactory<ContainerConfiguration> configuration )
		{
			this.configuration = configuration;
		}

		public override IServiceProvider Create()
		{
			var context = new CompositionFactory( configuration ).Create();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), new RecursionAwareServiceProvider( primary ), DefaultStoreServiceProvider.Instance );
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
