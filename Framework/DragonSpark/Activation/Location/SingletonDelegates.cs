using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Activation.Location
{
	public class SingletonDelegates : SingletonDelegates<Func<object>>
	{
		public static SingletonDelegates Default { get; } = new SingletonDelegates();
		SingletonDelegates() : this( SingletonProperties.Default ) {}
		public SingletonDelegates( IParameterizedSource<Type, PropertyInfo> source ) : base( source.ToSourceDelegate(), SingletonDelegateCache.Default.Get ) {}
		// public SingletonDelegates( ISpecification<SingletonRequest> specification, Func<PropertyInfo, Func<object>> source ) : base( specification, source ) {}
	}
}