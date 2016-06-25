using DragonSpark.Activation;
using DragonSpark.Aspects;
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

	public class Specification<TTarget, TProperty> : GuardedSpecificationBase<IServiceProvider>
	{
		public static Specification<TTarget, TProperty> Instance { get; } = new Specification<TTarget, TProperty>();

		public override bool IsSatisfiedBy( IServiceProvider parameter ) => 
			parameter.Get<IProvideValueTarget>().With( target => target.TargetObject is TTarget && target.TargetProperty is TProperty )
			;
	}

	[AutoValidation.GenericFactory]
	public abstract class MarkupPropertyFactoryBase : FactoryWithSpecificationBase<IServiceProvider, IMarkupProperty>, IMarkupPropertyFactory
	{
		protected MarkupPropertyFactoryBase( ISpecification<IServiceProvider> specification ) : base( specification ) {}
	}

	public abstract class MarkupPropertyFactoryBase<TTarget, TProperty> : MarkupPropertyFactoryBase
	{
		protected MarkupPropertyFactoryBase() : base( Specification<TTarget, TProperty>.Instance ) {}

		public sealed override IMarkupProperty Create( IServiceProvider parameter )
		{
			var target = parameter.Get<IProvideValueTarget>();
			var result = target != null ? Create( (TTarget)target.TargetObject, (TProperty)target.TargetProperty ) : null;
			return result;
		}

		protected abstract IMarkupProperty Create( TTarget targetObject, TProperty targetProperty );
	}
}
