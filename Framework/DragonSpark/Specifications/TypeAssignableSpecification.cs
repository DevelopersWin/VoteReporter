using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Specifications
{
	public sealed class TypeAssignableSpecification<T> : DelegatedSpecification<Type>
	{
		public static ISpecification<Type> Default { get; } = new TypeAssignableSpecification<T>();
		TypeAssignableSpecification() : base( TypeAssignableSpecification.Delegates.Get( typeof(T) ) ) {}
	}

	/*public sealed class TypeAssignableSpecification : DelegatedSpecification<Type>
	{
		public static IParameterizedSource<Type, Func<Type, bool>> Delegates { get; } = new Cache<Type, Func<Type, bool>>( type => Defaults.Get( type ).ToDelegate() );
		public static IParameterizedSource<Type, ISpecification<Type>> Defaults { get; } = new Cache<Type, ISpecification<Type>>( type => new TypeAssignableSpecification( type ).ToCachedSpecification() );
		TypeAssignableSpecification( Type targetType ) : base( targetType.Adapt().IsAssignableFrom ) {}
	}*/

	public sealed class TypeAssignableSpecification : SpecificationCache<Type>
	{
		public static TypeAssignableSpecification Default { get; } = new TypeAssignableSpecification();
		public static IParameterizedSource<Type, Func<Type, bool>> Delegates { get; } = Default.To( DelegateCoercer.Default ).ToCache();
		TypeAssignableSpecification() : base( type => new DelegatedSpecification<Type>( type.Adapt().IsAssignableFrom ).ToCachedSpecification() ) {}
	}

	public class SpecificationCache<T> : SpecificationCache<T, T> where T : class
	{
		public SpecificationCache( Func<T, ISpecification<T>> create ) : base( create ) {}
	}

	public class SpecificationCache<TKey, TSpecification> : Cache<TKey, ISpecification<TSpecification>> where TKey : class
	{
		public SpecificationCache( Func<TKey, ISpecification<TSpecification>> create ) : base( create ) {}

		/*sealed class Cache : ParameterizedSourceBase<TKey, ISpecification<TSpecification>>
		{
			readonly Func<TSpecification, bool> factory;
			public Cache( Func<TSpecification, bool> factory )
			{
				this.factory = factory;
			}

			public override ISpecification<TSpecification> Get( TKey parameter ) => new DelegatedSpecification<TSpecification>( factory ).ToCachedSpecification();
		}*/

		public sealed class DelegateCoercer : ParameterizedSourceBase<ISpecification<TSpecification>, Func<TSpecification, bool>>
		{
			public static DelegateCoercer Default { get; } = new DelegateCoercer();
			DelegateCoercer() {}

			public override Func<TSpecification, bool> Get( ISpecification<TSpecification> parameter ) => parameter.ToDelegate();
		}
	}

	/*public class SpecificationDelegateCache<TKey, TSpecification> : Cache<TKey, Func<TSpecification, bool>> where TKey : class
	{
		public SpecificationDelegateCache( IParameterizedSource<TKey, ISpecification<TSpecification>> source ) : base( source.To( Factory.Default ).Get ) {}

		
	}*/


}