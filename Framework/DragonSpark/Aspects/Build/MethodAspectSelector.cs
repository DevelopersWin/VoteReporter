using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class MethodAspectSelector<T> : AspectSelectorBase where T : IAspect
	{
		public MethodAspectSelector( IMethods store ) : base( TypeAssignableSpecification.Defaults.Get( store.ReferencedType ).Coerce( AsTypeCoercer.Default ).And( MethodAspectFactory<T>.Default ).ToDelegate(), store.To( MethodAspectFactory<T>.Default ).Get ) {}
	}

	public abstract class AspectSelectorBase : SpecificationParameterizedSource<TypeInfo, AspectInstance>, IAspectSelector
	{
		//protected AspectSelectorBase( Type supportedType, Func<TypeInfo, AspectInstance> factory ) : this( TypeAssignableSpecification.Delegates.Get( supportedType ).Get, factory ) {}
		protected AspectSelectorBase( Func<TypeInfo, bool> specification, Func<TypeInfo, AspectInstance> factory ) : base( specification, factory ) {}
	}
}