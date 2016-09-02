using DragonSpark.Activation.Location;
using DragonSpark.Application;
using DragonSpark.Composition;
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
		readonly static string[] Suffixes = { "Source", "Factory" };

		public static ISource<SourceTypes> Default { get; } = new Scope<SourceTypes>( Sources.Factory.GlobalCache( () => new SourceTypes() ) );
		SourceTypes() : base( new Factory().Get ) {}
		
		sealed class Factory : ParameterizedSourceBase<LocateTypeRequest, Type>
		{
			readonly ImmutableArray<SourceTypeRequest> requests;

			public Factory() : this( Requests.DefaultNested.CreateMany( ApplicationTypes.Default.Get().AsEnumerable() ) ) {}

			Factory( ImmutableArray<SourceTypeRequest> requests )
			{
				this.requests = requests;
			}

			public override Type Get( LocateTypeRequest parameter )
			{
				var candidates = requests.Introduce( parameter, tuple => tuple.Item1.Name == tuple.Item2.Name && tuple.Item2.RequestedType.Adapt().IsAssignableFrom( tuple.Item1.ResultType ) ).ToArray();
				var conventions = Suffixes.Introduce( parameter.RequestedType.Name, tuple => string.Concat( tuple.Item1, tuple.Item2 ) ).ToArray();
				var item = 
					candidates.Introduce( conventions, info => info.Item2.Contains( info.Item1.RequestedType.Name ) ).Only()
					??
					candidates.Introduce( parameter, arg => arg.Item1.ResultType == arg.Item2.RequestedType ).FirstOrDefault()
					??
					candidates.FirstOrDefault();

				var result = item?.RequestedType;
				return result;
			}

			sealed class Requests : ParameterizedSourceBase<Type, SourceTypeRequest>
			{
				readonly static Func<Type, Type> Results = ResultTypes.Default.ToSourceDelegate();
				readonly static ISpecification<Type> Specification = Activation.Defaults.Instantiable.And( Defaults.KnownSourcesSpecification, ContainsExportSpecification.Default, new DelegatedSpecification<Type>( type => Results( type ) != typeof(object) ) );

				public static IParameterizedSource<Type, SourceTypeRequest> DefaultNested { get; } = new Requests().Apply( Specification );
				Requests() {}

				public override SourceTypeRequest Get( Type parameter ) => 
					new SourceTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Results( parameter ) );
			}
		}
	}
}