using DragonSpark.TypeSystem.Generics;
using System;

namespace DragonSpark.Aspects.Validation
{
	abstract class GenericParameterProfileFactoryBase : GenericInvocationFactory<object, IParameterValidationAdapter>
	{
		protected GenericParameterProfileFactoryBase( Type genericTypeDefinition, Type owningType, string methodName ) : base( genericTypeDefinition, owningType, methodName ) {}
	}
}