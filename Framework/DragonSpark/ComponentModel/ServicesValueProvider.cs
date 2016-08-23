using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.ComponentModel
{
	public class ServicesValueProvider : ValueProvider<Type>
	{
		public ServicesValueProvider( Func<PropertyInfo, Type> convert ) : this( convert, GlobalServiceProvider.GetService<object> ) {}

		public ServicesValueProvider( Func<PropertyInfo, Type> convert, Func<Type, object> create ) : base( convert, create ) {}

		public class Converter : ParameterizedSourceBase<PropertyInfo, Type>
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