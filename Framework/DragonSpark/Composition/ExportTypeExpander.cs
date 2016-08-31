using DragonSpark.Sources;
using DragonSpark.Sources.Delegates;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Composition
{
	sealed class ExportTypeExpander : ParameterizedSourceBase<Type, IEnumerable<Type>>
	{
		public static ExportTypeExpander Default { get; } = new ExportTypeExpander();
		ExportTypeExpander() {}

		public override IEnumerable<Type> Get( Type parameter )
		{
			yield return parameter;
			var provider = Activator.Default.Sourced().ToDelegate();
			var sourceType = SourceTypeLocator.Default.Get( parameter );
			if ( sourceType != null )
			{
				yield return ResultTypes.Default.Get( sourceType );
				yield return ParameterizedSourceDelegates.Sources.Get( provider ).Get( sourceType )?.GetType() ?? SourceDelegates.Sources.Get( provider ).Get( sourceType )?.GetType();
			}
		}
	}
}