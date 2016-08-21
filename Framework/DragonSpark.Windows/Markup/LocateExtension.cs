using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using Moq;
using PostSharp.Patterns.Contracts;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using Activator = DragonSpark.Activation.Activator;
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
	public class SourceExtension : MarkupExtensionBase
	{
		public SourceExtension() {}

		public SourceExtension( [Required]ISource instance )
		{
			Instance = instance;
		}

		[Required]
		public ISource Instance { [return: Required]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => Instance.Get();
	}

	public class MockFactory : ValidatedParameterizedSourceBase<Type, object>
	{
		public static MockFactory Default { get; } = new MockFactory();

		MockFactory() : base( Specification.Default ) {}

		class Specification : SpecificationBase<Type>
		{
			public static Specification Default { get; } = new Specification();

			public override bool IsSatisfiedBy( Type parameter ) => parameter.IsInterface || !parameter.IsSealed;
		}

		public override object Get( Type parameter )
		{
			var type = typeof(Mock<>).MakeGenericType( parameter );
			var result = Activator.Activate<Mock>( type ).Object;
			return result;
		}
	}

	public class StringDesignerValueFactory : ValidatedParameterizedSourceBase<Type, object>
	{
		public static StringDesignerValueFactory Default { get; } = new StringDesignerValueFactory();

		public StringDesignerValueFactory() : base( TypeAssignableSpecification<string>.Default ) {}

		public override object Get( Type parameter ) => parameter.AssemblyQualifiedName;
	}

	public class DesignTimeValueProvider : CompositeFactory<Type, object>
	{
		public static DesignTimeValueProvider Default { get; } = new DesignTimeValueProvider();
		DesignTimeValueProvider() : base( SpecialValues.DefaultOrEmpty, MockFactory.Default.ToSourceDelegate(), StringDesignerValueFactory.Default.ToSourceDelegate() ) {}
	}

	public class MarkupValueSetterFactory : CompositeFactory<IServiceProvider, IMarkupProperty>, IMarkupPropertyFactory
	{
		public static MarkupValueSetterFactory Default { get; } = new MarkupValueSetterFactory();

		MarkupValueSetterFactory() : base( 
			DependencyPropertyMarkupPropertyFactory.Default, 
			CollectionMarkupPropertyFactory.Default, 
			PropertyInfoMarkupPropertyFactory.Default, 
			FieldInfoMarkupPropertyFactory.Default ) {}
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

		protected MarkupExtensionBase() : this( MarkupValueSetterFactory.Default.ToSourceDelegate(), DesignTimeValueProvider.Default.ToSourceDelegate() ) {}

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

	/*public class SourceExtension : MarkupExtensionBase
	{
		readonly ISource store;

		public SourceExtension( ISource store )
		{
			this.store = store;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => store.Get();
	}*/

	public class FactoryTypeExtension : MarkupExtensionBase
	{
		public FactoryTypeExtension( [Required]Type factoryType )
		{
			FactoryType = factoryType;
		}

		public Type FactoryType { get; set; }

		/*[Required, Service]
		SourceFactory SourceTypeFactory { [return: Required]get; set; }*/

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => SourceFactory.Default.Get( FactoryType );
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

		[Service, Required]
		IServiceProvider Locator { [return: NotNull]get; set; }

		// public Collection<PropertySetter> Properties { get; } = new Collection<PropertySetter>();

		protected override object GetValue( MarkupServiceProvider serviceProvider )
		{
			var result = Locator.GetService( Type/*, BuildName*/ );
			/*result.As<ISupportInitialize>( x => x.BeginInit() );
			result.With( x => Properties.Each( y => x.GetType().GetProperty( y.PropertyName ).With( z => y.Apply( z, x ) ) ) );
			result.As<ISupportInitialize>( x => x.EndInit() );*/
			return result;
		}
	}
}