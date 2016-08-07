using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation
{
	public sealed class IsSourceSpecification : AdapterSpecificationBase
	{
		public static ISpecification<Type> Instance { get; } = new IsSourceSpecification().Cached();
		IsSourceSpecification() : base( typeof(ISource<>), typeof(ISource), typeof(IFactory<>), typeof(IFactory) ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Adapters.IsAssignableFrom( parameter );
	}

	//public sealed class AllKnownSources

	public sealed class IsParameterizedSourceSpecification : AdapterSpecificationBase
	{
		public static ISpecification<Type> Instance { get; } = new IsParameterizedSourceSpecification().Cached();
		IsParameterizedSourceSpecification() : base( typeof(IParameterizedSource<,>), typeof(IParameterizedSource), typeof(IFactory<,>), typeof(IFactoryWithParameter) ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Adapters.Select( adapter => adapter.Type ).Any( parameter.Adapt().IsGenericOf );
	}

	public abstract class AdapterSpecificationBase : SpecificationBase<Type>
	{
		protected AdapterSpecificationBase( params Type[] types ) : this( types.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		AdapterSpecificationBase( ImmutableArray<TypeAdapter> adapters )
		{
			Adapters = adapters;
		}

		protected ImmutableArray<TypeAdapter> Adapters { get; }
	}

	public sealed class SourceInterfaces : FactoryCache<Type, Type>
	{
		readonly static Func<Type, bool> Specification = Defaults.KnownSourcesSpecification.IsSatisfiedBy;

		public static ICache<Type, Type> Instance { get; } = new SourceInterfaces();
		SourceInterfaces() {}

		protected override Type Create( Type parameter ) => parameter.Adapt().GetAllInterfaces().FirstOrDefault( Specification );
	}

	public sealed class ParameterTypes : TypeLocatorBase
	{
		public static ICache<Type, Type> Instance { get; } = new ParameterTypes();
		ParameterTypes() : base( typeof(Func<,>), typeof(IParameterizedSource<,>), typeof(IFactory<,>), typeof(ICommand<>) ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.First();
	}

	public sealed class ResultTypes : TypeLocatorBase
	{
		public static ICache<Type, Type> Instance { get; } = new ResultTypes();
		ResultTypes() : base( typeof(IParameterizedSource<,>), typeof(ISource<>), typeof(IFactory<,>), typeof(IFactory<>), typeof(Func<>), typeof(Func<,>) ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.Last();
	}

	public abstract class TypeLocatorBase : FactoryCache<Type, Type>
	{
		readonly ImmutableArray<TypeAdapter> adapters;
		readonly Func<TypeInfo, bool> isAssignable;
		readonly Func<Type[], Type> selector;

		protected TypeLocatorBase( params Type[] types ) : this( types.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		TypeLocatorBase( ImmutableArray<TypeAdapter> adapters )
		{
			this.adapters = adapters;
			isAssignable = IsAssignable;
			selector = Select;
		}

		protected override Type Create( Type parameter )
		{
			var result = parameter.Append( parameter.Adapt().GetAllInterfaces() )
				.AsTypeInfos()
				.Where( isAssignable )
				.Select( info => info.GenericTypeArguments )
				.Select( selector )
				.FirstOrDefault();
			return result;
		}

		bool IsAssignable( TypeInfo type ) => type.IsGenericType && adapters.IsAssignableFrom( type.GetGenericTypeDefinition() );

		protected abstract Type Select( IEnumerable<Type> genericTypeArguments );
	}

	public class ProjectedFactory<TFrom, TTo> : ProjectedFactory<object, TFrom, TTo>
	{
		public ProjectedFactory( Func<TFrom, TTo> convert ) : base( convert ) {}
	}

	public class ProjectedFactory<TBase, TFrom, TTo> where TFrom : TBase
	{
		readonly Func<TFrom, TTo> convert;

		public ProjectedFactory( Func<TFrom, TTo> convert )
		{
			this.convert = convert;
		}

		public virtual TTo Create( TBase parameter ) => parameter is TFrom ? convert( (TFrom)parameter ) : default(TTo);
	}

	public sealed class SourceTypes : EqualityReferenceCache<LocateTypeRequest, Type>
	{
		public static ISource<SourceTypes> Instance { get; } = new CachedScope<SourceTypes>( () => new SourceTypes() );
		SourceTypes() : base( new Factory().Create ) {}
		
		sealed class Factory :  FactoryBase<LocateTypeRequest, Type>
		{
			readonly ImmutableArray<FactoryTypeRequest> types;

			public Factory() : this( Requests.Instance.CreateMany( ApplicationParts.Instance.Get().Types.AsEnumerable() ) ) {}

			Factory( ImmutableArray<FactoryTypeRequest> types )
			{
				this.types = types;
			}

			public override Type Create( LocateTypeRequest parameter )
			{
				var candidates = types.Introduce( parameter, tuple => tuple.Item1.Name == tuple.Item2.Name && tuple.Item1.ResultType.Adapt().IsAssignableFrom( tuple.Item2.RequestedType ) ).ToArray();
				var item = 
					candidates.Introduce( $"{parameter.RequestedType.Name}Source", info => info.Item1.RequestedType.Name == info.Item2 ).Only()
					??
					candidates.Introduce( $"{parameter.RequestedType.Name}Factory", info => info.Item1.RequestedType.Name == info.Item2 ).Only()
					??
					candidates.Introduce( parameter, arg => arg.Item1.ResultType == arg.Item2.RequestedType ).FirstOrDefault()
					??
					candidates.FirstOrDefault();

				var result = item?.RequestedType;
				return result;
			}

			sealed class Requests : FactoryBase<Type, FactoryTypeRequest>
			{
				readonly static Func<Type, Type> Results = ResultTypes.Instance.ToDelegate();

				public static Requests Instance { get; } = new Requests();
				Requests() : base( CanInstantiateSpecification.Instance.And( Defaults.KnownSourcesSpecification, IsExportSpecification.Instance.Cast<Type>( type => type.GetTypeInfo() ), new DelegatedSpecification<Type>( type => Results( type ) != typeof(object) ) ) ) {}

				public override FactoryTypeRequest Create( Type parameter ) => 
					new FactoryTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Results( parameter ) );
			}
		}
	}
}