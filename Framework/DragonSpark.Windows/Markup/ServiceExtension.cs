using DragonSpark.ComponentModel;
using PostSharp.Patterns.Contracts;
using System;
using Type = System.Type;

namespace DragonSpark.Windows.Markup
{
	/*public class RootExtension : MarkupExtensionBase
	{
		protected override object GetValue( MarkupServiceProvider serviceProvider ) => serviceProvider.Get<IRootObjectProvider>().RootObject;
	}*/

	/*public class ServiceProviderTransformer : TransformerBase<IServiceProvider>
	{
		protected override IServiceProvider CreateItem( IServiceProvider parameter )
		{
			var target = parameter.Get<IProvideValueTarget>();
			var result = new MarkupServiceProvider( parameter, target.TargetObject, target.TargetProperty, propertyType );
			return result;
		}
	}*/

	/*public class SourceExtension : MarkupExtensionBase
	{
		readonly ISource store;

		public SourceExtension( ISource store )
		{
			this.store = store;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => store.Get();
	}*/

	// [ContentProperty( nameof(Properties) )]
	public class ServiceExtension : MarkupExtensionBase
	{
		public ServiceExtension() {}

		public ServiceExtension( Type type )
		{
			Type = type;
		}

		[Required]
		public Type Type { [return: NotNull]get; set; }

		[Service, Required]
		public IServiceProvider Locator { [return: NotNull]get; set; }

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