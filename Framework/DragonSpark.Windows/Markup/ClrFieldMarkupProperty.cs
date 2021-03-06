﻿using System.Reflection;

namespace DragonSpark.Windows.Markup
{
	public class ClrFieldMarkupProperty : ClrMemberMarkupProperty<FieldInfo>
	{
		public ClrFieldMarkupProperty( object target, FieldInfo targetProperty ) : base( targetProperty, x => targetProperty.SetValue( target, x ), () => targetProperty.GetValue( target ) ) {}
	}
}