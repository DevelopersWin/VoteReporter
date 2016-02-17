using System;
using System.Linq;
using System.Windows.Markup;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Windows.Markup
{
	public class MarkupValueContext : IServiceProvider, IProvideValueTarget
	{
		readonly static Type[] Types = { typeof(MarkupValueContext), typeof(IProvideValueTarget) };

		readonly IServiceProvider inner;

		public MarkupValueContext( [Required]IServiceProvider inner, [Required]object targetObject, object targetProperty, [Required]Type propertyType )
		{
			this.inner = inner;
			TargetObject = targetObject;
			TargetProperty = targetProperty;
			PropertyType = propertyType;
		}

		public object TargetObject { get; }
		public object TargetProperty { get; }
		public Type PropertyType { get; }

		public virtual object GetService( Type serviceType ) => Types.Any( type => type.IsAssignableFrom( serviceType ) ) ? this : inner.GetService( serviceType );
	}
}