﻿using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Validation
{
	sealed class AdapterLocator : ParameterizedSourceBase<IParameterValidationAdapter>
	{
		public static AdapterLocator Default { get; } = new AdapterLocator();
		AdapterLocator() : this( Profiles.Default.Get ) {}

		readonly Func<Type, IAutoValidationProfile> profileSource;

		AdapterLocator( Func<Type, IAutoValidationProfile> profileSource )
		{
			this.profileSource = profileSource;
		}

		public override IParameterValidationAdapter Get( object parameter )
		{
			var type = parameter.GetType();
			var result = profileSource( type )?.Get( parameter );
			if ( result != null )
			{
				return result;
			}

			throw new InvalidOperationException( $"Adapter not found for {type}." );
		}
	}
}