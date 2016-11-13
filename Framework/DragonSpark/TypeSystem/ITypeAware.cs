using System;

namespace DragonSpark.TypeSystem
{
	public interface ITypeAware
	{
		Type ReferencedType { get; }
	}
}