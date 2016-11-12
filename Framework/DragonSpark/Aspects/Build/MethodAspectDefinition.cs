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
		readonly Type referencedType;
		public MethodAspectDefinition( IMethods store ) : this( store.ReferencedType, store.To( MethodAspectFactory<T>.Default ).ToCache().ToDelegate() ) { }

		MethodAspectDefinition( Type referencedType, Func<Type, AspectInstance> store ) : base( 
			TypeAssignableSpecification.Defaults.Get( referencedType )
				.And( new DelegatedAssignedSpecification<Type, AspectInstance>( store ) )
				.Coerce( AsTypeCoercer.Default )
				.ToDelegate(), 
			store.Get
		)
		{
			this.referencedType = referencedType;
		}

		protected override bool Validate( TypeInfo parameter )
		{
			var validate = base.Validate( parameter );
			
			return validate;
		}

		public override AspectInstance Get( TypeInfo parameter )
		{
			var aspectInstance = base.Get( parameter );
			return aspectInstance;
		}
	}
}