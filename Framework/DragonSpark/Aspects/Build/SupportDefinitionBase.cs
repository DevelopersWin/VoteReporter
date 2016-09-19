using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public abstract class SupportDefinitionBase : ISupportDefinition
	{
		readonly Func<Type, bool> specification;
		readonly ImmutableArray<IAspectInstanceLocator> locators;

		protected SupportDefinitionBase( Func<Type, bool> specification, params IAspectInstanceLocator[] locators )
		{
			this.specification = specification;
			this.locators = locators.ToImmutableArray();
		}

		public bool IsSatisfiedBy( Type parameter ) => specification( parameter );

		public IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var locator in locators )
			{
				var instance = locator.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}
}