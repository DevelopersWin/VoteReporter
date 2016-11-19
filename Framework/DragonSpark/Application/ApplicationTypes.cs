using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Application
{
	public sealed class ApplicationTypes : DelegatedItemSource<Type>
	{
		public static IItemSource<Type> Default { get; } = new ApplicationTypes();
		ApplicationTypes() : base( () => ApplicationParts.Default.Get()?.Types.AsEnumerable() ?? Items<Type>.Default ) {}
	}
}