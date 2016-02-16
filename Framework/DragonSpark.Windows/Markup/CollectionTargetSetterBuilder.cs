using DragonSpark.Extensions;
using System;
using System.Collections;
using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	public class CollectionTargetSetterBuilder : MarkupTargetValueSetterFactory<IList, object>
	{
		public static CollectionTargetSetterBuilder Instance { get; } = new CollectionTargetSetterBuilder();

		protected override bool Handles( IProvideValueTarget service ) => base.Handles( service ) && service.TargetObject.GetType().Adapt().GetInnerType() != null;

		protected override IMarkupTargetValueSetter Create( IList targetObject, object targetProperty ) => new CollectionSetter( targetObject );

		protected override Type GetPropertyType( IList target, object property ) => target.GetType();
	}
}