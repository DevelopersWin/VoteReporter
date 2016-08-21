using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class IsSourceSpecification : AdapterSpecificationBase
	{
		public static ISpecification<Type> Default { get; } = new IsSourceSpecification().Cached();
		IsSourceSpecification() : base( typeof(ISource<>), typeof(ISource) ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Adapters.IsAssignableFrom( parameter );
	}

	//public sealed class AllKnownSources

	public sealed class IsParameterizedSourceSpecification : AdapterSpecificationBase
	{
		public static ISpecification<Type> Default { get; } = new IsParameterizedSourceSpecification().Cached();
		IsParameterizedSourceSpecification() : base( typeof(IParameterizedSource<,>), typeof(IParameterizedSource) ) {}

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

		public static ICache<Type, Type> Default { get; } = new SourceInterfaces();
		SourceInterfaces() {}

		protected override Type Create( Type parameter ) => parameter.Adapt().GetAllInterfaces().FirstOrDefault( Specification );
	}

	public sealed class ParameterTypes : TypeLocatorBase
	{
		public static ICache<Type, Type> Default { get; } = new ParameterTypes();
		ParameterTypes() : base( typeof(Func<,>), typeof(IParameterizedSource<,>), typeof(ICommand<>) ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.First();
	}

	public sealed class ResultTypes : TypeLocatorBase
	{
		public static ICache<Type, Type> Default { get; } = new ResultTypes();
		ResultTypes() : base( typeof(IParameterizedSource<,>), typeof(ISource<>), typeof(Func<>), typeof(Func<,>) ) {}

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

	public class ProjectedSource<TFrom, TTo> : ProjectedSource<object, TFrom, TTo>
	{
		public ProjectedSource( Func<TFrom, TTo> convert ) : base( convert ) {}
	}

	public class ProjectedSource<TBase, TFrom, TTo> : ParameterizedSourceBase<TBase, TTo> where TFrom : TBase
	{
		readonly Func<TFrom, TTo> convert;

		public ProjectedSource( Func<TFrom, TTo> convert )
		{
			this.convert = convert;
		}

		public override TTo Get( TBase parameter ) => parameter is TFrom ? convert( (TFrom)parameter ) : default(TTo);
	}

	public sealed class SourceTypes : EqualityReferenceCache<LocateTypeRequest, Type>
	{
		public static ISource<SourceTypes> Default { get; } = new Scope<SourceTypes>( Sources.Factory.Global( () => new SourceTypes() ) );
		SourceTypes() : base( new Factory().Get ) {}
		
		sealed class Factory : ParameterizedSourceBase<LocateTypeRequest, Type>
		{
			readonly ImmutableArray<FactoryTypeRequest> types;

			public Factory() : this( Requests.Default.CreateMany( ApplicationParts.Default.Get().Types.AsEnumerable() ) ) {}

			Factory( ImmutableArray<FactoryTypeRequest> types )
			{
				this.types = types;
			}

			public override Type Get( LocateTypeRequest parameter )
			{
				var candidates = types.Introduce( parameter, tuple => tuple.Item1.Name == tuple.Item2.Name && tuple.Item2.RequestedType.Adapt().IsAssignableFrom( tuple.Item1.ResultType ) ).ToArray();
				var conventions = $"{parameter.RequestedType.Name}Source".Append( $"{parameter.RequestedType.Name}Factory" ).ToArray();
				var item = 
					candidates.Introduce( conventions, info => info.Item2.Contains( info.Item1.RequestedType.Name ) ).Only()
					??
					candidates.Introduce( parameter, arg => arg.Item1.ResultType == arg.Item2.RequestedType ).FirstOrDefault()
					??
					candidates.FirstOrDefault();

				var result = item?.RequestedType;
				return result;
			}

			sealed class Requests : ValidatedParameterizedSourceBase<Type, FactoryTypeRequest>
			{
				readonly static Func<Type, Type> Results = ResultTypes.Default.ToSourceDelegate();

				public static Requests Default { get; } = new Requests();
				Requests() : base( Defaults.ActivateSpecification.And( Defaults.KnownSourcesSpecification, Defaults.IsExportSpecification, new DelegatedSpecification<Type>( type => Results( type ) != typeof(object) ) ) ) {}

				public override FactoryTypeRequest Get( Type parameter ) => 
					new FactoryTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Results( parameter ) );
			}
		}
	}
}