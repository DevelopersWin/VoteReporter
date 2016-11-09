using System.Collections.Generic;
using DragonSpark.Aspects.Build;
using DragonSpark.TypeSystem;

namespace DragonSpark.Aspects.Definitions
{
	public interface ITypeDefinition : ITypeAware, IEnumerable<IMethods> {}
}