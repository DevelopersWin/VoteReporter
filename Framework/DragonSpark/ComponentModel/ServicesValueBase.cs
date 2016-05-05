using DragonSpark.Activation;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class LocateAttribute : DefaultValueBase
	{
		public LocateAttribute() : this( null ) { }

		public LocateAttribute( string name ) : this( null, name ) { }

		public LocateAttribute( Type locatedType, string name = null ) : base( t => new LocationValueProvider( new Converter( locatedType, name ).Create, Factory.Instance.Create ) ) { }
		
		public class LocationValueProvider : ValueProvider<LocateTypeRequest>
		{
			public LocationValueProvider( Func<PropertyInfo, LocateTypeRequest> convert, Func<LocateTypeRequest, object> create ) : base( convert, create ) {}
		}

		public class Factory : FactoryBase<LocateTypeRequest, object>
		{
			public static Factory Instance { get; } = new Factory();

			readonly ServiceLocatorProvider locator;

			Factory() : this( Services.Get<IServiceLocator> ) {}

			Factory( [Required]ServiceLocatorProvider locator )
			{
				this.locator = locator;
			}

			protected override object CreateItem( LocateTypeRequest parameter )
			{
				var serviceLocator = locator();
				var instance = serviceLocator?.GetInstance( parameter.RequestedType, parameter.Name );
				var result = instance ?? Services.Get( parameter.RequestedType );
				return result;
			}
		}

		public class Converter : FactoryBase<PropertyInfo, LocateTypeRequest>
		{
			readonly Func<PropertyInfo, Type> type;
			readonly string name;

			public Converter( Type activatedType, string name ) : this( p => activatedType ?? p.PropertyType, name ) { }

			protected Converter( [Required]Func<PropertyInfo, Type> type, string name )
			{
				this.type = type;
				this.name = name;
			}

			protected override LocateTypeRequest CreateItem( PropertyInfo parameter ) => new LocateTypeRequest( type( parameter ), name );
		}
	}

	public abstract class ServicesValueBase : DefaultValueBase
	{
		protected ServicesValueBase( ServicesValueProvider.Converter converter ) : this( converter, Services.Get ) {}

		protected ServicesValueBase( ServicesValueProvider.Converter converter, Func<Type, object> creator ) : base( t => new ServicesValueProvider( converter.Create, creator ) ) {}

		protected ServicesValueBase( Func<object, IDefaultValueProvider> provider ) : base( provider ) {}
	}

	public class ServicesValueProvider : ValueProvider<Type>
	{
		public ServicesValueProvider( Func<PropertyInfo, Type> convert ) : this( convert, Services.Get ) {}

		public ServicesValueProvider( Func<PropertyInfo, Type> convert, Func<Type, object> create ) : base( convert, create ) {}

		public class Converter : FactoryBase<PropertyInfo, Type>
		{
			readonly Func<PropertyInfo, Type> type;

			public Converter( Type activatedType ) : this( p => activatedType ?? p.PropertyType ) { }

			public Converter( [Required]Func<PropertyInfo, Type> type )
			{
				this.type = type;
			}

			protected override Type CreateItem( PropertyInfo parameter ) => type( parameter );
		}
	}

	public class ValueProvider<TRequest> : IDefaultValueProvider
	{
		readonly Func<PropertyInfo, TRequest> convert;
		readonly Func<TRequest, object> create;

		public ValueProvider( [Required]Func<PropertyInfo, TRequest> convert, [Required]Func<TRequest, object> create )
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