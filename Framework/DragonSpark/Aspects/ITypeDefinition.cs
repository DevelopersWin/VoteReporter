using DragonSpark.Aspects.Build;
using DragonSpark.TypeSystem;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	public interface ITypeDefinition : ITypeAware, IEnumerable<IMethods> {}
}