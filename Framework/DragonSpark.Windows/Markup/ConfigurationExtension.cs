using DragonSpark.Activation.FactoryModel;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	public class ConfigurationExtension : MarkupExtensionBase
	{
		readonly string key;
		readonly Func<PropertyReference, string> factory = MemberInfoKeyFactory.Instance.Create;

		public ConfigurationExtension() {}

		public ConfigurationExtension( [NotEmpty]string key ) : this( key, MemberInfoKeyFactory.Instance.Create ) {}

		public ConfigurationExtension( [NotEmpty]string key, [Required]Func<PropertyReference, string> factory )
		{
			this.key = key;
			this.factory = factory;
		}

		[Required, Service]
		IConfigurationRegistry Registry { [return: Required]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider )
		{
			var name = key ?? factory( serviceProvider.Property.Reference );
			var result = Registry.Get( name ) ?? FromTarget( serviceProvider );
			return result;
		}

		object FromTarget( MarkupServiceProvider serviceProvider )
		{
			var adjusted = new PropertyReference( serviceProvider.TargetObject.GetType(), serviceProvider.Property.Reference.PropertyType, serviceProvider.Property.Reference.PropertyName );
			var name = factory( adjusted );
			var result = Registry.Get( name );
			return result;
		}
	}

	/*public class MemberInfoFromPropertyFactory : FirstFromParameterFactory<object, MemberInfo>
	{
		public class PropertyInfoFactory : FactoryBase<object, MemberInfo>
		{
			public static PropertyInfoFactory Instance { get; } = new PropertyInfoFactory();

			protected override MemberInfo CreateItem( object parameter ) => parameter;
		}

		public MemberInfoFromPropertyFactory() : base( PropertyInfoFactory.Instance )
		{
		}
	}*/

	public class MemberInfoKeyFactory : FactoryBase<PropertyReference, string>
	{
		public static MemberInfoKeyFactory Instance { get; } = new MemberInfoKeyFactory();

		protected override string CreateItem( PropertyReference parameter ) => $"{parameter.DeclaringType.FullName}::{parameter.PropertyName}";
	}

	[MarkupExtensionReturnType( typeof(string) )]
	public abstract class ConfigurationKeyExtension : MarkupExtensionBase
	{
		readonly string key;

		protected ConfigurationKeyExtension( [NotEmpty]string key )
		{
			this.key = key;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => key;
	}

	
	public class MemberInfoKeyExtension : ConfigurationKeyExtension
	{
		public MemberInfoKeyExtension( [Required]Type type, string member ) : this( type.GetMember( member ).First() ) {}

		public MemberInfoKeyExtension( [Required]MemberInfo member ) : base( MemberInfoKeyFactory.Instance.Create( PropertyReference.New( member ) ) ) {}
	}
}