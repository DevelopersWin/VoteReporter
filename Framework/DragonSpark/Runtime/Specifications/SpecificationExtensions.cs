using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Reflection;

namespace DragonSpark.Runtime.Specifications
{
	public static class Projections
	{
		public static Func<Type, MemberInfo> MemberType { get; } = info => info.GetTypeInfo();
	}

	public static class SpecificationExtensions
	{
		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => new InverseSpecification<T>( @this );
		
		/*public static Func<T, bool> Inverse<T>( this Func<T, bool> @this ) => Inversed<T>.Default.Get( @this );
		sealed class Inversed<T> : Cache<Func<T, bool>, Func<T, bool>>
		{
			public static Inversed<T> Default { get; } = new Inversed<T>();
			Inversed() : base( factory => new Converter( factory ).Create ) {}

			class Converter : FactoryBase<T, bool>
			{
				readonly Func<T, bool> @from;
				public Converter( Func<T, bool> @from )
				{
					this.@from = @from;
				}

				public override bool Create( T parameter ) => !from( parameter );
			}
		}
*/
		public static ISpecification<T> Or<T>( this ISpecification<T> @this, params ISpecification<T>[] others ) 
			=> new AnySpecification<T>( @this.Append( others ).Fixed() );

		public static ISpecification<T> And<T>( this ISpecification<T> @this, params ISpecification<T>[] others ) 
			=> new AllSpecification<T>( @this.Append( others ).Fixed() );

		// public static ISpecification<T> Cast<T>( this ISpecification @this ) => @this.Cast( Delegates<T>.Object );

		public static ISpecification<TDestination> Project<TDestination, TOrigin>( this ISpecification<TOrigin> @this, Func<TDestination, TOrigin> projection ) => new ProjectedSpecification<TOrigin, TDestination>( @this.IsSatisfiedBy, projection );

		public static ISpecification<TTo> Cast<TFrom, TTo>( this ISpecification<TFrom> @this ) where TFrom : TTo => new CastedSpecification<TFrom, TTo>( @this );

		public static Func<T, bool> ToDelegate<T>( this ISpecification<T> @this ) => DelegateCache<T>.Default.Get( @this );
		class DelegateCache<T> : Cache<ISpecification<T>, Func<T, bool>>
		{
			public static DelegateCache<T> Default { get; } = new DelegateCache<T>();
			DelegateCache() : base( specification => specification.IsSatisfiedBy ) {}
		}

		public static ISpecification<T> Cached<T>( this ISpecification<T> @this ) => Cache<T>.Default.Get( @this );
		class Cache<T> : Cache<ISpecification<T>, ISpecification<T>>
		{
			public static Cache<T> Default { get; } = new Cache<T>();
			Cache() : base( specification => new DelegatedSpecification<T>( specification.ToDelegate().Fix() ) ) {}
		}
	}
}