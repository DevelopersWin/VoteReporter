using System;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class TypeDefinitionFormatter : IFormattable
	{
		readonly ITypeDefinition definition;
		public TypeDefinitionFormatter( ITypeDefinition definition )
		{
			this.definition = definition;
		}

		public string ToString( string format = null, IFormatProvider formatProvider = null ) => definition.ReferencedType.FullName;
	}
}