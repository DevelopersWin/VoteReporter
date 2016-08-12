using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Linq;

namespace DragonSpark.Runtime.Specifications
{
	public static class SpecificationExtensions
	{
		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => new InverseSpecification( @this ).Cast<T>();
		
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
		public static ISpecification<T> Or<T>( this ISpecification<T> @this, params ISpecification[] others ) 
			=> new AnySpecification<T>( @this.Append( others.Select( specification => specification.Cast<T>() ) ).Fixed() );

		public static ISpecification<T> And<T>( this ISpecification<T> @this, params ISpecification[] others ) 
			=> new AllSpecification<T>( @this.Append( others.Select( specification => specification.Cast<T>() ) ).Fixed() );

		public static ISpecification<T> Cast<T>( this ISpecification @this ) => @this.Cast( Delegates<T>.Object );

		public static ISpecification<T> Cast<T>( this ISpecification @this, Func<T, object> projection ) => @this as ISpecification<T> ?? new DecoratedSpecification<T>( @this, projection );

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