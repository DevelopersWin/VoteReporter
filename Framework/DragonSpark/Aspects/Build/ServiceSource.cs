using DragonSpark.Sources;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;

namespace DragonSpark.Aspects.Build
{
	public class ServiceSource<T> : DelegatedSource<T> where T : class, IService
	{
		public static ServiceSource<T> Default { get; } = new ServiceSource<T>();
		ServiceSource() : base( () => PostSharpEnvironment.CurrentProject.GetService<T>() ) {}
	}

	public class AspectRepositoryService : SuppliedSource<IAspectRepositoryService>, IAspectRepositoryService
	{
		public static AspectRepositoryService Default { get; } = new AspectRepositoryService();
		AspectRepositoryService() {}

		public IAspectInstance[] GetAspectInstances( object declaration ) => Get().GetAspectInstances( declaration );

		public bool HasAspect( object declaration, Type aspectType ) => Get().HasAspect( declaration, aspectType );

		public event EventHandler AspectDiscoveryCompleted
		{
			add { Get().AspectDiscoveryCompleted += value; }
			remove { Get().AspectDiscoveryCompleted -= value; }
		}
	}
}