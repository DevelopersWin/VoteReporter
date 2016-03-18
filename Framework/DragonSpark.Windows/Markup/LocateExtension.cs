using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ServiceLocation;
using Moq;
using PostSharp.Patterns.Contracts;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using Type = System.Type;

namespace DragonSpark.Windows.Markup
{
	[ContentProperty( nameof( Properties ) )]
	public class AmbientExtension : MarkupExtensionBase
	{
		readonly Type type;

		public AmbientExtension( [Required]Type type )
		{
			this.type = type;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => Ambient.GetCurrent( type );
	}

	public class RootExtension : MarkupExtensionBase
	{
		protected override object GetValue( MarkupServiceProvider serviceProvider ) => serviceProvider.Get<IRootObjectProvider>().RootObject;
	}

	[ContentProperty( nameof(Instance) )]
	public class FactoryExtension : MarkupExtensionBase
	{
		public FactoryExtension() {}

		public FactoryExtension( [Required]IFactory instance )
		{
			Instance = instance;
		}

		[Required]
		public IFactory Instance { [return: Required]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => Instance.Create();
	}

	public class DesignTimeValueProvider : FactoryBase<Type, object>
	{
		public static DesignTimeValueProvider Instance { get; } = new DesignTimeValueProvider();

		protected override object CreateItem( Type parameter )
		{
			var enumerableType = parameter.Adapt().GetEnumerableType();
			var items = enumerableType == null;
			var name = items ? nameof(Default<object>.Item) : nameof(Default<object>.Items);
			var targetType = enumerableType ?? parameter;
			var property = typeof(Default<>).MakeGenericType( targetType ).GetProperty( name );
			var result = property.GetValue( null ) ?? CreateMock( targetType );
			return result;
		}

		static object CreateMock( Type targetType )
		{
			var type = typeof(Mock<>).MakeGenericType( targetType );
			var result = Services.Get<Mock>( type ).Object;
			return result;
		}
	}

	public class MarkupValueSetterFactory : FirstFromParameterFactory<IServiceProvider, IMarkupProperty>, IMarkupPropertyFactory
	{
		public static MarkupValueSetterFactory Instance { get; } = new MarkupValueSetterFactory();

		MarkupValueSetterFactory() : base( 
			DependencyPropertyMarkupPropertyFactory.Instance, 
			CollectionMarkupPropertyFactory.Instance, 
			PropertyInfoMarkupPropertyFactory.Instance, 
			FieldInfoMarkupPropertyFactory.Instance ) {}
	}

	/*public class ServiceProviderTransformer : TransformerBase<IServiceProvider>
	{
		protected override IServiceProvider CreateItem( IServiceProvider parameter )
		{
			var target = parameter.Get<IProvideValueTarget>();
			var result = new MarkupServiceProvider( parameter, target.TargetObject, target.TargetProperty, propertyType );
			return result;
		}
	}*/

	public abstract class MarkupExtensionBase : MarkupExtension
	{
		readonly IMarkupPropertyFactory factory;
		readonly Func<Type, object> designTimeFactory;

		protected MarkupExtensionBase() : this( MarkupValueSetterFactory.Instance, DesignTimeValueProvider.Instance.Create ) {}

		protected MarkupExtensionBase( [Required]IMarkupPropertyFactory factory, [Required]Func<Type, object> designTimeFactory )
		{
			this.factory = factory;
			this.designTimeFactory = designTimeFactory;
		}

		public override object ProvideValue( IServiceProvider serviceProvider )
		{
			var designMode = DesignerProperties.GetIsInDesignMode( new DependencyObject() );
			try
			{
				// Retrieve target information
				var service = serviceProvider.Get<IProvideValueTarget>();
				if ( service?.TargetObject != null )
				{
					// In a template the TargetObject is a SharedDp (internal WPF class)
					// In that case, the markup extension itself is returned to be re-evaluated later
					switch ( service.TargetObject.GetType().FullName )
					{
						case "System.Windows.SharedDp":
							return this;
						default:
							return factory.Create( serviceProvider ).With( property =>
							{
								var value = designMode ? designTimeFactory( property.Reference.PropertyType ) : GetValue( new MarkupServiceProvider( serviceProvider, service.TargetObject, property ) );

								var result = service.TargetProperty == null ? property.SetValue( value ) : value;
								return result;
							} );
					}
				}
				return null;
			}
			catch ( Exception e )
			{
				var exception = designMode ? new Exception( e.ToString() ) : e;
				throw exception;
			}
		}

		protected abstract object GetValue( MarkupServiceProvider serviceProvider );
	}

	public class ValueExtension : MarkupExtensionBase
	{
		readonly IValue value;

		public ValueExtension( [Required]IValue value )
		{
			this.value = value;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => value.Item;
	}

	public class FactoryTypeExtension : MarkupExtensionBase
	{
		public FactoryTypeExtension( [Required]Type factoryType )
		{
			FactoryType = factoryType;
		}

		public Type FactoryType { get; set; }

		[Required, Service]
		InstanceFromFactoryTypeFactory Factory { [return: Required]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => Factory.Create( FactoryType );
	}

	[ContentProperty( nameof(Properties) )]
	public class LocateExtension : MarkupExtensionBase
	{
		public LocateExtension() {}

		public LocateExtension( Type type, string buildName = null )
		{
			Type = type;
			BuildName = buildName;
		}

		[Required]
		public Type Type { [return: NotNull]get; set; }

		public string BuildName { get; set; }

		[Locate, Required]
		IServiceLocator Locator { [return: NotNull]get; set; }

		// public Collection<PropertySetter> Properties { get; } = new Collection<PropertySetter>();

		protected override object GetValue( MarkupServiceProvider serviceProvider )
		{
			var result = Locator.GetInstance( Type, BuildName );
			/*result.As<ISupportInitialize>( x => x.BeginInit() );
			result.With( x => Properties.Each( y => x.GetType().GetProperty( y.PropertyName ).With( z => y.Apply( z, x ) ) ) );
			result.As<ISupportInitialize>( x => x.EndInit() );*/
			return result;
		}
	}
}