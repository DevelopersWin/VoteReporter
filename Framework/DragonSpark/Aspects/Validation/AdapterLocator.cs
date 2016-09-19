using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	sealed class AdapterLocator : ParameterizedSourceBase<IParameterValidationAdapter>
	{
		public static AdapterLocator Default { get; } = new AdapterLocator();
		AdapterLocator() : this( Defaults.Factories ) {}

		readonly IEnumerable<KeyValuePair<TypeAdapter, Func<object, IParameterValidationAdapter>>> pairs;

		AdapterLocator( IEnumerable<KeyValuePair<TypeAdapter, Func<object, IParameterValidationAdapter>>> pairs )
		{
			this.pairs = pairs;
		}

		public override IParameterValidationAdapter Get( object parameter )
		{
			var type = parameter.GetType();
			foreach ( var pair in pairs )
			{
				if ( pair.Key.IsAssignableFrom( type ) )
				{
					var result = pair.Value( parameter );
					if ( result != null )
					{
						return result;
					}
				}
			}

			throw new InvalidOperationException( $"Adapter not found for {type}." );
		}
	}
}