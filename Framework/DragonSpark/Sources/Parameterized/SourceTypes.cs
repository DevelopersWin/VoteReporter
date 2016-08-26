using DragonSpark.Activation.Location;
using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class SourceTypes : EqualityReferenceCache<LocateTypeRequest, Type>
	{
		public static ISource<SourceTypes> Default { get; } = new Scope<SourceTypes>( Sources.Factory.Global( () => new SourceTypes() ) );
		SourceTypes() : base( new Factory().Get ) {}
		
		sealed class Factory : ParameterizedSourceBase<LocateTypeRequest, Type>
		{
			readonly ImmutableArray<SourceTypeRequest> types;

			public Factory() : this( Requests.DefaultNested.CreateMany( ApplicationParts.Default.Get().Types.AsEnumerable() ) ) {}

			Factory( ImmutableArray<SourceTypeRequest> types )
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

			sealed class Requests : ValidatedParameterizedSourceBase<Type, SourceTypeRequest>
			{
				readonly static Func<Type, Type> Results = ResultTypes.Default.ToSourceDelegate();

				public static Requests DefaultNested { get; } = new Requests();
				Requests() : base( Defaults.ActivateSpecification.And( Defaults.KnownSourcesSpecification, Defaults.ContainsExportSpecification, new DelegatedSpecification<Type>( type => Results( type ) != typeof(object) ) ) ) {}

				public override SourceTypeRequest Get( Type parameter ) => 
					new SourceTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Results( parameter ) );
			}
		}
	}
}