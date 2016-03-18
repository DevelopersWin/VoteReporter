using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using System;
using System.Windows.Markup;

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
		public static Specification<TTarget, TProperty> Instance { get; } = new Specification<TTarget, TProperty>();

		protected override bool Verify( IServiceProvider parameter ) => 
			parameter.Get<IProvideValueTarget>().With( target => target.TargetObject is TTarget && target.TargetProperty is TProperty )
			;
	}

	public abstract class MarkupPropertyFactoryBase : FactoryBase<IServiceProvider, IMarkupProperty>, IMarkupPropertyFactory
	{
		protected MarkupPropertyFactoryBase( ISpecification<IServiceProvider> specification ) : base( specification ) {}
	}

	public abstract class MarkupPropertyFactoryBase<TTarget, TProperty> : MarkupPropertyFactoryBase
	{
		protected MarkupPropertyFactoryBase() : base( Specification<TTarget, TProperty>.Instance ) {}

		// protected MarkupPropertyFactoryBase( ISpecification<IServiceProvider> specification ) : base( specification ) {}

		protected sealed override IMarkupProperty CreateItem( IServiceProvider parameter ) => 
			parameter.Get<IProvideValueTarget>().With( target => Create( (TTarget)target.TargetObject, (TProperty)target.TargetProperty ) );

		protected abstract IMarkupProperty Create( TTarget targetObject, TProperty targetProperty );
	}
}
