using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Definitions
{
	public abstract class TypeDefinitionWithPrimaryMethodBase : TypeDefinition
	{
		protected TypeDefinitionWithPrimaryMethodBase( IMethods methods ) : base( methods.ReferencedType, methods )
		{
			PrimaryMethod = methods;
		}

		public IMethods PrimaryMethod { get; }
	}
}