using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public interface IAspectSelector : ISpecificationParameterizedSource<TypeInfo, AspectInstance>/*, ITypeAware*/ {}

	public sealed class AspectSelection : ParameterizedItemSourceBase<ITypeDefinition, IAspectSelector>
	{
		readonly Func<ITypeDefinition, IEnumerable<IAspectSelector>> selectors;

		public AspectSelection( Func<ITypeDefinition, IEnumerable<IAspectSelector>> selectors )
		{
			this.selectors = selectors;
		}

		public override IEnumerable<IAspectSelector> Yield( ITypeDefinition parameter ) => selectors( parameter );
	}
}