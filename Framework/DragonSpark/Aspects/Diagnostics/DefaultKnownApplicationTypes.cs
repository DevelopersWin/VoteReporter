using System;
using DragonSpark.Sources;
using DragonSpark.TypeSystem;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class DefaultKnownApplicationTypes : ItemSource<Type>
	{
		public static DefaultKnownApplicationTypes Default { get; } = new DefaultKnownApplicationTypes();
		DefaultKnownApplicationTypes() : base( typeof(DiagnosticsConfiguration), typeof(MethodFormatter), typeof(TypeFormatter), typeof(TypeDefinitionFormatter) ) {}
	}
}