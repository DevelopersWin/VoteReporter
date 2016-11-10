using DragonSpark.Aspects.Build;
using System;

namespace DragonSpark.Aspects.Definitions
{
	public class ValidatedTypeDefinition : TypeDefinition, IValidatedTypeDefinition
	{
		public ValidatedTypeDefinition( Type referencedType, string execution ) : this( new Methods( referencedType, execution ) ) {}
		public ValidatedTypeDefinition( IMethods execution ) : this( execution.ReferencedType, execution ) {}
		public ValidatedTypeDefinition( Type referencedType, IMethods execution ) : this( referencedType, GenericSpecificationTypeDefinition.Default.PrimaryMethod, execution ) {}
		public ValidatedTypeDefinition( Type referencedType, string validation, string execution ) : this( referencedType, new Methods( referencedType, validation ), new Methods( referencedType, execution ) ) {}
		public ValidatedTypeDefinition( Type referencedType, IMethods validation, IMethods execution ) : base( referencedType, validation, execution )
		{
			Validation = validation;
			Execution = execution;
		}

		public IMethods Validation { get; }
		public IMethods Execution { get; }
	}
}