using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	public sealed class AspectInstances : ParameterizedSourceBase<Type, IEnumerable<AspectInstance>>
	{
		readonly ImmutableArray<IAspectInstanceLocator> locators;

		public AspectInstances( params IAspectInstances[] instances ) : this( instances.Concat().Fixed() ) {}

		public AspectInstances( params IAspectInstanceLocator[] locators )
		{
			this.locators = locators.ToImmutableArray();
		}

		public override IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var source in locators )
			{
				var instance = source.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}
}