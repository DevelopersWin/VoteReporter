using System;
using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.Sources;

namespace DragonSpark.Setup
{
	public sealed class Instances : Scope<IServiceRepository>
	{
		public static ISource<IServiceRepository> Default { get; } = new Instances();
		Instances() : base( Factory.Global( () => new InstanceServiceProvider( SingletonLocator.Default ) ) ) {}

		public static T Get<T>( Type type ) => Default.Get().Get<T>( type );
	}
}