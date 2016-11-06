using DragonSpark.Aspects.Build;
using System;

namespace DragonSpark.Aspects
{
	public class ValidatedTypeDefinition : TypeDefinition, IValidatedTypeDefinition
	{
		public ValidatedTypeDefinition( Type referencedType, string execution ) : this( new MethodStore( referencedType, execution ) ) {}
		public ValidatedTypeDefinition( IMethodStore execution ) : this( execution.ReferencedType, execution ) {}
		public ValidatedTypeDefinition( Type referencedType, IMethodStore execution ) : this( referencedType, GenericSpecificationTypeDefinition.Default.Method, execution ) {}
		public ValidatedTypeDefinition( Type referencedType, string validation, string execution ) : this( referencedType, new MethodStore( referencedType, validation ), new MethodStore( referencedType, execution ) ) {}
		public ValidatedTypeDefinition( Type referencedType, IMethodStore validation, IMethodStore execution ) : base( referencedType, validation, execution )
		{
			Validation = validation;
			Execution = execution;
		}

		public IMethodStore Validation { get; }
		public IMethodStore Execution { get; }
	}
}