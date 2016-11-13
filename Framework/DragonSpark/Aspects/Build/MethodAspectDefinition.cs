using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Build
{
	public class MethodAspectDefinition<T> : AspectDefinitionBase where T : IAspect
	{
		public MethodAspectDefinition( IMethods store ) : this( store.ReferencedType, store.To( MethodAspectFactory<T>.Default ).ToCache().ToDelegate() ) { }
		MethodAspectDefinition( Type referencedType, Func<Type, AspectInstance> store ) : base( 
			TypeAssignableSpecification.Default.Get( referencedType )
				.And( new DelegatedAssignedSpecification<Type, AspectInstance>( store ) )
				.Coerce( AsTypeCoercer.Default )
				.ToDelegate(), 
			store.Get
		) {}
	}
}