using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
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
using Activator = DragonSpark.Activation.Activator;
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

		protected override object GetValue( MarkupValueContext serviceProvider ) => Ambient.GetCurrent( type );
	}

	public class RootExtension : MarkupExtensionBase
	{
		protected override object GetValue( MarkupValueContext serviceProvider ) => serviceProvider.Get<IRootObjectProvider>().RootObject;
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

		protected override object GetValue( MarkupValueContext serviceProvider ) => Instance.Create();
	}

	public class DesignTimeValueProvider : FactoryBase<MarkupValueContext, object>
	{
		public static DesignTimeValueProvider Instance { get; } = new DesignTimeValueProvider();

		protected override object CreateItem( MarkupValueContext parameter )
		{
			var type = parameter.PropertyType;
			var enumerableType = type.Adapt().GetEnumerableType();
			var items = enumerableType == null;
			var name = items ? nameof(Default<object>.Item) : nameof(Default<object>.Items);
			var targetType = enumerableType ?? type;
			var property = typeof(Default<>).MakeGenericType( targetType ).GetProperty( name );
			var result = property.GetValue( null ) ?? CreateMock( targetType );
			return result;
		}

		static object CreateMock( Type targetType )
		{
			var type = typeof(Mock<>).MakeGenericType( targetType );
			var result = Activator.GetCurrent().Activate<Mock>( type ).Object;
			return result;
		}
	}

	public abstract class MarkupExtensionBase : MarkupExtension
	{
		readonly IFactory<MarkupValueContext, object> designTimeFactory;

		readonly static IMarkupTargetValueSetterBuilder[] DefaultBuilders = 
		{
			DependencyPropertyMarkupTargetValueSetterBuilder.Instance,
			CollectionTargetSetterBuilder.Instance,
			PropertyInfoMarkupTargetValueSetterBuilder.Instance,
			FieldInfoMarkupTargetValueSetterBuilder.Instance
		};

		protected MarkupExtensionBase() : this( DefaultBuilders, DesignTimeValueProvider.Instance ) {}

		protected MarkupExtensionBase( [Required]IMarkupTargetValueSetterBuilder[] builders, [Required]IFactory<MarkupValueContext, object> designTimeFactory )
		{
			this.designTimeFactory = designTimeFactory;
			Builders = builders;
		}

		protected IMarkupTargetValueSetterBuilder[] Builders { get; }

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
							return Builders.WithFirst( builder => builder.Handles( service ), builder =>
							{
								var context = Prepare( serviceProvider, service, builder.GetPropertyType( service ) );
								var value = designMode ? designTimeFactory.Create( context ) : GetValue( context );

								var result = context.TargetProperty == null ? builder.Create( service ).SetValue( value ) : value;
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

		protected abstract object GetValue( MarkupValueContext serviceProvider );

		protected virtual MarkupValueContext Prepare( IServiceProvider serviceProvider, IProvideValueTarget target, Type propertyType )
			=> new MarkupValueContext( serviceProvider, target.TargetObject, target.TargetProperty, propertyType );
	}

	public class ValueExtension : MarkupExtensionBase
	{
		readonly IValue value;

		public ValueExtension( [Required]IValue value )
		{
			this.value = value;
		}

		protected override object GetValue( MarkupValueContext serviceProvider ) => value.Item;
	}

	public class FactoryTypeExtension : MarkupExtensionBase
	{
		public FactoryTypeExtension( [Required]Type factoryType )
		{
			FactoryType = factoryType;
		}

		public Type FactoryType { get; set; }

		protected override object GetValue( MarkupValueContext serviceProvider ) => Factory.From( FactoryType );
	}

	[ContentProperty( nameof(Properties) )]
	public class LocateExtension : MarkupExtensionBase
	{
		public LocateExtension()
		{}

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

		/*[BuildUp]
		protected override object GetValue( IServiceProvider serviceProvider )
		{
			var result = Locator.GetInstance( Type, BuildName );
			result.As<ISupportInitialize>( x => x.BeginInit() );
			result.With( x => Properties.Each( y => x.GetType().GetProperty( y.PropertyName ).With( z => y.Apply( z, x ) ) ) );
			result.As<ISupportInitialize>( x => x.EndInit() );
			return result;
		}*/

		// public Collection<PropertySetter> Properties { get; } = new Collection<PropertySetter>();

		[BuildUp]
		protected override object GetValue( MarkupValueContext serviceProvider )
		{
			var result = Locator.GetInstance( Type, BuildName );
			/*result.As<ISupportInitialize>( x => x.BeginInit() );
			result.With( x => Properties.Each( y => x.GetType().GetProperty( y.PropertyName ).With( z => y.Apply( z, x ) ) ) );
			result.As<ISupportInitialize>( x => x.EndInit() );*/
			return result;
		}
	}
}