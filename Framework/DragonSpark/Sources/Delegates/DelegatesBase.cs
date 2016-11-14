using DragonSpark.Activation;
using DragonSpark.Expressions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using DragonSpark.TypeSystem.Generics;
using System;

namespace DragonSpark.Sources.Delegates
{
	public abstract class DelegatesBase : CacheWithImplementedFactoryBase<Type, Delegate>
	{
		protected DelegatesBase( IActivator source, string name ) : this( Parameterized.Extensions.ToDelegate( source ), Common.Assigned, name ) {}
		protected DelegatesBase( Func<Type, object> locator, ISpecification<Type> specification, string name ) : base( specification )
		{
			Locator = locator;
			Methods = GetType().GetFactory( name );
		}

		protected Func<Type, object> Locator { get; }
		protected IGenericMethodContext<Invoke> Methods { get; }
	}
}