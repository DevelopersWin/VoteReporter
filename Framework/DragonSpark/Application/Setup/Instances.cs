using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Application.Setup
{
	public sealed class Instances : Scope<IServiceRepository>
	{
		public static ISource<IServiceRepository> Default { get; } = new Instances();
		Instances() : base( Factory.Global( () => new InstanceRepository( Singletons.Default ) ) ) {}

		public static T Get<T>( Type type ) => Default.Get().Get<T>( type );
	}
}