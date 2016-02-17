using DragonSpark.Activation.FactoryModel;
using System;
using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	public interface IMarkupTargetValueSetter : IDisposable
	{
		object SetValue( object value );
	}

	public abstract class MarkupTargetValueSetterFactory<TTarget, TProperty> : FactoryBase<IProvideValueTarget, IMarkupTargetValueSetter>, IMarkupTargetValueSetterBuilder
	{
		bool IMarkupTargetValueSetterBuilder.Handles( IProvideValueTarget service ) => Handles( service );

		protected virtual bool Handles( IProvideValueTarget service ) => service.TargetObject is TTarget && ( typeof(TProperty) == typeof(object) || service.TargetProperty is TProperty );

		public Type GetPropertyType( IProvideValueTarget parameter ) => GetPropertyType( (TTarget)parameter.TargetObject, (TProperty)parameter.TargetProperty );

		protected abstract Type GetPropertyType( TTarget target, TProperty property );

		protected sealed override IMarkupTargetValueSetter CreateItem( IProvideValueTarget parameter ) => Create( (TTarget)parameter.TargetObject, (TProperty)parameter.TargetProperty );

		protected abstract IMarkupTargetValueSetter Create( TTarget targetObject, TProperty targetProperty );
	}
}
