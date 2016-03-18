using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;

namespace DragonSpark.Windows.Markup
{
	public class ClrFieldMarkupProperty : ClrMemberMarkupProperty<FieldInfo>
	{
		public ClrFieldMarkupProperty( object targetObject, FieldInfo targetProperty ) : base( targetProperty, x => targetProperty.SetValue( targetObject, x ), () => targetProperty.GetValue( targetObject ) ) {}
	}

	public class ClrPropertyMarkupProperty : ClrMemberMarkupProperty<PropertyInfo>
	{
		public ClrPropertyMarkupProperty( object targetObject, PropertyInfo targetProperty ) : base( targetProperty, x => targetProperty.SetValue( targetObject, x ), () => targetProperty.GetValue( targetObject ) ) {}
	}

	public abstract class ClrMemberMarkupProperty<T> : MarkupPropertyBase where T : MemberInfo
	{
		readonly Action<object> setter;
		readonly Func<object> getter;

		protected ClrMemberMarkupProperty( [Required]T targetProperty, [Required]Action<object> setter, [Required]Func<object> getter ) : base( PropertyReference.New( targetProperty ) )
		{
			this.setter = setter;
			this.getter = getter;
		}

		protected override object OnGetValue() => getter();

		protected override object Apply( object value )
		{
			setter( value );
			return null;
		}
	}
}
