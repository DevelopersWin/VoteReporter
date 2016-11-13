using System;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public class AspectRepositoryService : ServiceSource<IAspectRepositoryService>, IAspectRepositoryService
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