using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using JetBrains.Annotations;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Windows.Markup
{
	public class ConfigurationExtension : MarkupExtensionBase
	{
		readonly string key;

		public ConfigurationExtension() {}

		public ConfigurationExtension( string key )
		{
			this.key = key;
		}

		[Required, Service, UsedImplicitly]
		IValueStore Store { [return: Required] get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider )
		{
			var name = key ?? MemberInfoKeyFactory.Default.Get( serviceProvider.Property.Reference );
			var result = Store.Get( name ) ?? FromTarget( serviceProvider );
			return result;
		}

		object FromTarget( MarkupServiceProvider serviceProvider )
		{
			var adjusted = new PropertyReference( serviceProvider.TargetObject.GetType(), serviceProvider.Property.Reference.PropertyType, serviceProvider.Property.Reference.PropertyName );
			var name = MemberInfoKeyFactory.Default.Get( adjusted );
			var result = Store.Get( name );
			return result;
		}
	}

	/*public class MemberInfoFromPropertyFactory : FirstFromParameterFactory<object, MemberInfo>
	{
		public class PropertyInfoFactory : Factory<object, MemberInfo>
		{
			public static PropertyInfoFactory Default { get; } = new PropertyInfoFactory();

			protected override MemberInfo CreateItem( object parameter ) => parameter;
		}

		public MemberInfoFromPropertyFactory() : base( PropertyInfoFactory.Default )
		{
		}
	}*/
}