using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	sealed class AdapterLocator : ParameterizedSourceBase<IParameterValidationAdapter>
	{
		public static AdapterLocator Default { get; } = new AdapterLocator();
		AdapterLocator() : this( AdapterSources.Default.Get ) {}

		readonly Func<Type, IAdapterSource> factorySource;

		AdapterLocator( Func<Type, IAdapterSource> factorySource )
		{
			this.factorySource = factorySource;
		}

		public override IParameterValidationAdapter Get( object parameter )
		{
			var other = parameter.GetType();
			var adapter = factorySource( other )?.Get( parameter );
			if ( adapter != null )
			{
				return adapter;
			}

			throw new InvalidOperationException( $"Adapter not found for {other}." );
		}
	}
}