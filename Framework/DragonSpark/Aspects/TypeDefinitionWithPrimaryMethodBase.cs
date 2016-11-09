using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects
{
	public abstract class TypeDefinitionWithPrimaryMethodBase : TypeDefinition
	{
		protected TypeDefinitionWithPrimaryMethodBase( IMethods method ) : base( method.ReferencedType, method )
		{
			Method = method;
		}

		public IMethods Method { get; }
	}
}