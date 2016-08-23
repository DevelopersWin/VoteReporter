using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.ComponentModel
{
	public abstract class ServicesValueBase : DefaultValueBase
	{
		protected ServicesValueBase( ServicesValueProvider.Converter converter ) : this( converter, GlobalServiceProvider.GetService<object> ) {}

		protected ServicesValueBase( ServicesValueProvider.Converter converter, Func<Type, object> creator ) : base( t => new ServicesValueProvider( converter.Get, creator ) ) {}

		protected ServicesValueBase( Func<object, IDefaultValueProvider> provider ) : base( provider ) {}
	}

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

	public class ValueProvider<TRequest> : IDefaultValueProvider
	{
		readonly Func<PropertyInfo, TRequest> convert;
		readonly Func<TRequest, object> create;

		public ValueProvider( Func<PropertyInfo, TRequest> convert, Func<TRequest, object> create )
		{
			this.convert = convert;
			this.create = create;
		}

		public object GetValue( DefaultValueParameter parameter )
		{
			var request = convert( parameter.Metadata );
			var result = create( request );
			return result;
		}
	}
}