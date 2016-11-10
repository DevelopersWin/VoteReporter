using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Definitions
{
	public abstract class TypeDefinitionWithPrimaryMethodBase : TypeDefinition
	{
		protected TypeDefinitionWithPrimaryMethodBase( IMethods primaryMethod ) : base( primaryMethod.ReferencedType, primaryMethod )
		{
			PrimaryMethod = primaryMethod;
		}

		public IMethods PrimaryMethod { get; }
	}
}