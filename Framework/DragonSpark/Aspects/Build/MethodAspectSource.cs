using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class MethodAspectSource<T> : AspectSourceBase where T : IAspect
	{
		public MethodAspectSource( IMethods store ) : base( store.ReferencedType, store.To( MethodAspectFactory<T>.Default ).Get ) {}
	}

	public abstract class AspectSourceBase : SpecificationParameterizedSource<TypeInfo, AspectInstance>, IAspectSource
	{
		protected AspectSourceBase( Type supportedType, Func<Type, AspectInstance> factory ) : this( TypeAssignableSpecification.Defaults.Get( supportedType ).ToDelegate(), factory ) {}
		protected AspectSourceBase( Func<Type, bool> specification, Func<Type, AspectInstance> factory ) : base( specification.Get, factory.Get ) {}
	}
}