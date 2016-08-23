using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Windows.Markup;
using DragonSpark.Specifications;

namespace DragonSpark.Windows.Markup
{
	public interface IMarkupProperty
	{
		PropertyReference Reference { get; }
		
		object GetValue();

		object SetValue( object value );
	}

	public class Specification<TTarget, TProperty> : SpecificationBase<IServiceProvider>
	{
		public static Specification<TTarget, TProperty> Default { get; } = new Specification<TTarget, TProperty>();

		public override bool IsSatisfiedBy( IServiceProvider parameter ) => 
			parameter.Get<IProvideValueTarget>().With( target => target.TargetObject is TTarget && target.TargetProperty is TProperty )
			;
	}

	public abstract class MarkupPropertyFactoryBase : ValidatedParameterizedSourceBase<IServiceProvider, IMarkupProperty>, IMarkupPropertyFactory
	{
		protected MarkupPropertyFactoryBase( ISpecification<IServiceProvider> specification ) : base( specification ) {}
	}

	public abstract class MarkupPropertyFactoryBase<TTarget, TProperty> : MarkupPropertyFactoryBase
	{
		protected MarkupPropertyFactoryBase() : base( Specification<TTarget, TProperty>.Default ) {}

		public sealed override IMarkupProperty Get( IServiceProvider parameter )
		{
			var target = parameter.Get<IProvideValueTarget>();
			var result = target != null ? Create( (TTarget)target.TargetObject, (TProperty)target.TargetProperty ) : null;
			return result;
		}

		protected abstract IMarkupProperty Create( TTarget targetObject, TProperty targetProperty );
	}
}
