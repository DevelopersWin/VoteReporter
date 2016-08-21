using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using DragonSpark.Sources.Parameterized;
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

		public ConfigurationExtension() {}

		public ConfigurationExtension( string key )
		{
			this.key = key;
		}

		[Required, Service]
		IValueStore Registry { [return: Required]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider )
		{
			var name = key ?? MemberInfoKeyFactory.Default.Get( serviceProvider.Property.Reference );
			var result = Registry.Get( name ) ?? FromTarget( serviceProvider );
			return result;
		}

		object FromTarget( MarkupServiceProvider serviceProvider )
		{
			var adjusted = new PropertyReference( serviceProvider.TargetObject.GetType(), serviceProvider.Property.Reference.PropertyType, serviceProvider.Property.Reference.PropertyName );
			var name = MemberInfoKeyFactory.Default.Get( adjusted );
			var result = Registry.Get( name );
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

	public class MemberInfoKeyFactory : ParameterizedSourceBase<PropertyReference, string>
	{
		public static MemberInfoKeyFactory Default { get; } = new MemberInfoKeyFactory();

		public override string Get( PropertyReference parameter ) => $"{parameter.DeclaringType.FullName}::{parameter.PropertyName}";
	}

	[MarkupExtensionReturnType( typeof(string) )]
	public abstract class ConfigurationKeyExtension : MarkupExtensionBase
	{
		readonly string key;

		protected ConfigurationKeyExtension( string key )
		{
			this.key = key;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => key;
	}

	
	public class MemberInfoKeyExtension : ConfigurationKeyExtension
	{
		public MemberInfoKeyExtension( Type type, string member ) : this( type.GetMember( member ).First() ) {}

		public MemberInfoKeyExtension( MemberInfo member ) : base( MemberInfoKeyFactory.Default.Get( PropertyReference.New( member ) ) ) {}
	}
}