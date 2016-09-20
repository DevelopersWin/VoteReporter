using DragonSpark.Aspects.Build;
using System;

namespace DragonSpark.Aspects
{
	public class ValidatedComponentDefinition : Definition, IValidatedComponentDefinition
	{
		public ValidatedComponentDefinition( Type declaringType, string execution ) : this( declaringType, new MethodStore( declaringType, execution ) ) {}
		public ValidatedComponentDefinition( Type declaringType, IMethodStore execution ) : this( declaringType, GenericSpecificationDefinition.Default.Method, execution ) {}
		public ValidatedComponentDefinition( Type declaringType, string validation, string execution ) : this( declaringType, new MethodStore( declaringType, validation ), new MethodStore( declaringType, execution ) ) {}
		public ValidatedComponentDefinition( Type declaringType, IMethodStore validation, IMethodStore execution ) : base( declaringType, validation, execution )
		{
			Validation = validation;
			Execution = execution;
		}

		public IMethodStore Validation { get; }
		public IMethodStore Execution { get; }
	}
}