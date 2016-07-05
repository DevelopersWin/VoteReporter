using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ServiceLocation;
using Moq;
using PostSharp.Patterns.Contracts;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using Type = System.Type;

namespace DragonSpark.Windows.Markup
{
	/*[ContentProperty( nameof( Properties ) )]
	public class AmbientExtension : MarkupExtensionBase
	{
		readonly Type type;

		public AmbientExtension( [Required]Type type )
		{
			this.type = type;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => AmbientStack.GetCurrent( type );
	}*/

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

	public class MockFactory : FactoryBase<Type, object>
	{
		public static MockFactory Instance { get; } = new MockFactory();

		public class Specification : GuardedSpecificationBase<Type>
		{
			public static Specification Instance { get; } = new Specification();

			public override bool IsSatisfiedBy( Type parameter ) => parameter.IsInterface || !parameter.IsSealed;
		}

		public MockFactory() : base( Specification.Instance ) {}

		public override object Create( Type parameter )
		{
			var type = typeof(Mock<>).MakeGenericType( parameter );
			var result = GlobalServiceProvider.Instance.Get<Mock>( type ).Object;
			return result;
		}
	}

	public class StringDesignerValueFactory : FactoryBase<Type, object>
	{
		public static StringDesignerValueFactory Instance { get; } = new StringDesignerValueFactory();

		public StringDesignerValueFactory() : base( TypeAssignableSpecification<string>.Instance ) {}

		public override object Create( Type parameter ) => parameter.AssemblyQualifiedName;
	}

	public class DesignTimeValueProvider : CompositeFactory<Type, object>
	{
		public static DesignTimeValueProvider Instance { get; } = new DesignTimeValueProvider();

		DesignTimeValueProvider() : base( SpecialValues.DefaultOrEmpty, MockFactory.Instance.ToDelegate(), StringDesignerValueFactory.Instance.ToDelegate() ) {}
	}

	public class MarkupValueSetterFactory : CompositeFactory<IServiceProvider, IMarkupProperty>, IMarkupPropertyFactory
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

	public class EvalExtension : MarkupExtensionBase
	{
		readonly object item;
		readonly string expression;
		
		public EvalExtension( [Required]object item, [NotEmpty]string expression )
		{
			this.item = item;
			this.expression = expression;
		}

		[Required, Service]
		IExpressionEvaluator Evaluator { [return: Required]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => Evaluator.Evaluate( item, expression );
	}

	public abstract class MarkupExtensionBase : MarkupExtension
	{
		readonly static ThreadLocal<DependencyObject> DependencyObject = new ThreadLocal<DependencyObject>( () => new DependencyObject() );

		readonly Func<IServiceProvider, IMarkupProperty> factory;
		readonly Func<Type, object> designTimeFactory;

		protected MarkupExtensionBase() : this( MarkupValueSetterFactory.Instance.ToDelegate(), DesignTimeValueProvider.Instance.ToDelegate() ) {}

		protected MarkupExtensionBase( [Required]Func<IServiceProvider, IMarkupProperty> factory, [Required]Func<Type, object> designTimeFactory )
		{
			this.factory = factory;
			this.designTimeFactory = designTimeFactory;
		}

		public override object ProvideValue( IServiceProvider serviceProvider )
		{
			var designMode = DesignerProperties.GetIsInDesignMode( DependencyObject.Value );
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
							var property = factory( serviceProvider );
							if ( property != null )
							{
								var value = designMode ? designTimeFactory( property.Reference.PropertyType ) : GetValue( new MarkupServiceProvider( serviceProvider, service.TargetObject, property ) );
								var result = service.TargetProperty == null ? property.SetValue( value ) : value;
								return result;
							}
							break;
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
		readonly IStore store;

		public ValueExtension( [Required]IStore store )
		{
			this.store = store;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => store.Value;
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