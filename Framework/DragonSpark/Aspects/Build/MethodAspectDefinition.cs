using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class MethodAspectDefinition<T> : AspectDefinitionBase where T : IAspect
	{
		public MethodAspectDefinition( IMethods store ) : this( store.ReferencedType, store.To( MethodAspectFactory<T>.Default ).ToCache().ToDelegate() ) { }

		MethodAspectDefinition( Type referencedType, Func<Type, AspectInstance> store ) : base( 
			TypeAssignableSpecification.Defaults.Get( referencedType )
				.And( new DelegatedAssignedSpecification<Type, AspectInstance>( store ) )
				.Coerce( AsTypeCoercer.Default )
				.ToDelegate(), 
			store.Get
		) {}
	}

	public abstract class AspectDefinitionBase : SpecificationParameterizedSource<TypeInfo, AspectInstance>, IAspectDefinition
	{
		//protected AspectSelectorBase( Type supportedType, Func<TypeInfo, AspectInstance> factory ) : this( TypeAssignableSpecification.Delegates.Get( supportedType ).Get, factory ) {}
		protected AspectDefinitionBase( Func<TypeInfo, bool> specification, Func<TypeInfo, AspectInstance> factory ) : base( specification, factory ) {}
	}
}