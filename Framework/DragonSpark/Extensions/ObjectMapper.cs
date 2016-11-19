using AutoMapper;
using AutoMapper.Configuration;
using DragonSpark.Activation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System;
using System.Runtime.InteropServices;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Extensions
{
	public sealed class ObjectMapper<T> : ParameterizedSourceCache<object, T, T> where T : class
	{
		public static ObjectMapper<T> Default { get; } = new ObjectMapper<T>();
		ObjectMapper() : this( ParameterConstructor<object, Mapper>.Default ) {}

		[UsedImplicitly]
		public ObjectMapper( Func<object, IParameterizedSource<T, T>> create ) : base( create ) {}

		public sealed class Mapper : ParameterizedSourceBase<T, T>
		{
			readonly object source;
			readonly Type sourceType;
			readonly IActivator activator;
			readonly IParameterizedSource<TypePair, IMapper> mappers;

			[UsedImplicitly]
			public Mapper( object source ) : this( source, source.GetType(), Activator.Default, Mappers.Default ) {}

			[UsedImplicitly]
			public Mapper( object source, Type sourceType, IActivator activator, IParameterizedSource<TypePair, IMapper> mappers )
			{
				this.source = source;
				this.sourceType = sourceType;
				this.activator = activator;
				this.mappers = mappers;
			}

			public override T Get( [Optional]T parameter )
			{
				var key = new TypePair( sourceType, parameter?.GetType() ?? ( typeof(T) == typeof(object) ? sourceType : typeof(T) ) );
				var destination = parameter ?? activator.Get( key.DestinationType );
				var result = mappers.Get( key ).Map( source, (T)destination );
				return result;
			}
		}
	}

	public sealed class Mappers : ExtendedDictionaryCache<TypePair, IMapper>
	{
		public static Mappers Default { get; } = new Mappers();
		Mappers() : base( Implementation.Instance.Get ) {}

		sealed class Implementation : ParameterizedSourceBase<TypePair, IMapper>
		{
			public static IParameterizedSource<TypePair, IMapper> Instance { get; } = new Implementation();
			Implementation() {}

			public override IMapper Get( TypePair parameter )
			{
				var expression = new MapperConfigurationExpression();
				expression.ForAllPropertyMaps( map => map.SourceMember == null || !map.DestinationPropertyType.IsAssignableFrom( map.SourceMember.GetMemberType() ), ( map, _ ) => map.Ignored = true );
				expression.CreateMap( parameter.SourceType, parameter.DestinationType, MemberList.Destination );
				expression.ForAllMaps( ( map, mappingExpression ) => mappingExpression.ForAllMembers( option => option.Condition( ( source, destination, sourceValue ) => sourceValue.IsAssigned() ) ) );
				var result = new MapperConfiguration( expression ).CreateMapper();
				return result;
			}
		}
	}
}