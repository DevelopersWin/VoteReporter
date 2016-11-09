using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class MethodAspectSelector<T> : AspectSelectorBase where T : IAspect
	{
		public MethodAspectSelector( IMethods store ) : base( store.ReferencedType, store.To( MethodAspectFactory<T>.Default ).Get ) {}
	}

	public abstract class AspectSelectorBase : SpecificationParameterizedSource<TypeInfo, AspectInstance>, IAspectSelector
	{
		protected AspectSelectorBase( Type supportedType, Func<Type, AspectInstance> factory ) : this( TypeAssignableSpecification.Defaults.Get( supportedType ).ToSpecificationDelegate(), factory ) {}
		protected AspectSelectorBase( Func<Type, bool> specification, Func<Type, AspectInstance> factory ) : base( specification.Get, factory.Get ) {}
	}
}