using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Sources.Delegates
{
	public abstract class DelegatesBase : FactoryCache<Type, Delegate>
	{
		protected DelegatesBase( Func<IServiceProvider> source, string name ) : this( source.Delegate<object>(), Specifications.Specifications.Assigned, name ) {}
		protected DelegatesBase( Func<Type, object> locator, ISpecification<Type> specification, string name ) : base( specification )
		{
			Locator = locator;
			Methods = GetType().Adapt().GenericFactoryMethods[ name ];
		}

		protected Func<Type, object> Locator { get; }
		protected IGenericMethodContext<Invoke> Methods { get; }
	}
}