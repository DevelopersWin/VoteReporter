using DragonSpark.Sources.Parameterized;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Defaults = DragonSpark.Activation.Location.Defaults;

namespace DragonSpark.ComponentModel
{
	public class ServicesValueProvider : ValueProvider<Type>
	{
		public ServicesValueProvider( Func<PropertyInfo, Type> convert ) : this( convert, Defaults.ServiceSource ) {}

		public ServicesValueProvider( Func<PropertyInfo, Type> convert, Func<Type, object> create ) : base( convert, create ) {}

		public sealed class Converter : ParameterizedSourceBase<PropertyInfo, Type>
		{
			readonly Func<PropertyInfo, Type> type;

			public Converter( [Optional]Type activatedType ) : this( p => activatedType ?? p.PropertyType ) { }

			public Converter( Func<PropertyInfo, Type> type )
			{
				this.type = type;
			}

			public override Type Get( PropertyInfo parameter ) => type( parameter );
		}
	}
}