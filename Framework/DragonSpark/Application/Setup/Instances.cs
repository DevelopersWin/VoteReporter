using DragonSpark.Activation;
using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Application.Setup
{
	public sealed class Instances : SingletonScope<IServiceRepository>, IServiceRepository
	{
		public static Instances Default { get; } = new Instances();
		Instances() : base( () => new InstanceRepository( SingletonLocator.Default, Constructor.Default ) ) {}

		object IParameterizedSource<Type, object>.Get( Type parameter ) => base.Get().Get( parameter );
		public object GetService( Type serviceType ) => base.Get().GetService( serviceType );
		public bool IsSatisfiedBy( Type parameter ) => base.Get().IsSatisfiedBy( parameter );
		public new ImmutableArray<object> Get() => base.Get().Get();
		public IEnumerator<object> GetEnumerator() => base.Get().GetEnumerator();
		public void Add( object instance ) => base.Get().Add( instance );
		public void Add( ServiceRegistration request ) => base.Get().Add( request );
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}