using DragonSpark.Activation.Location;
using DragonSpark.Sources;
using DragonSpark.Sources.Delegates;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;

namespace DragonSpark.Composition
{
	sealed class ExportTypeExpander : ParameterizedSourceBase<Type, IEnumerable<Type>>
	{
		public static ExportTypeExpander Default { get; } = new ExportTypeExpander();
		ExportTypeExpander() {}

		public override IEnumerable<Type> Get( Type parameter )
		{
			yield return parameter;
			var provider = SingletonLocator.Default.Sourced().ToDelegate();
			var sourceType = SourceTypeLocator.Default.Get( parameter );
			if ( sourceType != null )
			{
				yield return ResultTypes.Default.Get( sourceType );
				yield return ParameterizedSourceDelegates.Sources.Get( provider ).Get( sourceType )?.GetType() ?? SourceDelegates.Sources.Get( provider ).Get( sourceType )?.GetType();
			}
		}
	}
}