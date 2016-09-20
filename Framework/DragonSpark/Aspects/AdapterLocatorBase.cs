using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	public abstract class AdapterLocatorBase<T> : ParameterizedSourceBase<T>
	{
		readonly IEnumerable<ValueTuple<TypeAdapter, Func<object, T>>> pairs;

		protected AdapterLocatorBase( IEnumerable<ValueTuple<TypeAdapter, Func<object, T>>> pairs )
		{
			this.pairs = pairs;
		}

		public override T Get( object parameter )
		{
			var type = parameter.GetType();
			foreach ( var pair in pairs )
			{
				if ( pair.Item1.IsAssignableFrom( type ) )
				{
					var result = pair.Item2( parameter );
					if ( result != null )
					{
						return result;
					}
				}
			}

			throw new InvalidOperationException( $"{typeof(T).FullName} not found for {type}." );
		}
	}
}