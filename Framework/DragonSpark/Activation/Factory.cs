using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation
{
	public class IsFactorySpecification : AdapterSpecificationBase
	{
		public static IsFactorySpecification Instance { get; } = new IsFactorySpecification( ImmutableArray.Create( typeof(IFactory), typeof(IFactoryWithParameter) ) );

		public IsFactorySpecification( ImmutableArray<Type> types ) : base( types ) {}

		[Freeze]
		public override bool IsSatisfiedBy( Type parameter ) => Adapters.IsAssignableFrom( parameter );
	}

	public class IsGenericFactorySpecification : AdapterSpecificationBase
	{
		public static IsGenericFactorySpecification Instance { get; } = new IsGenericFactorySpecification( ImmutableArray.Create( typeof(IFactory<>), typeof(IFactory<,>) ) );

		public IsGenericFactorySpecification( ImmutableArray<Type> types ) : base( types ) {}

		[Freeze]
		public override bool IsSatisfiedBy( Type parameter ) => Adapters.Select( adapter => adapter.Type ).Any( parameter.Adapt().IsGenericOf );
	}

	public abstract class AdapterSpecificationBase : SpecificationBase<Type>
	{
		protected AdapterSpecificationBase( ImmutableArray<Type> types ) : this( types.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		protected AdapterSpecificationBase( ImmutableArray<TypeAdapter> adapters )
		{
			Adapters = adapters;
		}

		protected ImmutableArray<TypeAdapter> Adapters { get; }
	}

	// [AutoValidation( false )]
	public class FactoryInterfaceLocator : FactoryBase<Type, Type>
	{
		public static FactoryInterfaceLocator Instance { get; } = new FactoryInterfaceLocator();

		[Freeze]
		public override Type Create( Type parameter ) => parameter.Adapt().GetAllInterfaces().With( types => types.FirstOrDefault( IsGenericFactorySpecification.Instance.ToDelegate() ) ?? types.FirstOrDefault( IsFactorySpecification.Instance.ToDelegate() ) );
	}

	public class ParameterTypeLocator : TypeLocatorCacheBase
	{
		public static ICache<Type, Type> Instance { get; } = new ParameterTypeLocator( ImmutableArray.Create( typeof(Func<,>), typeof(IFactory<,>), typeof(ICommand<>) ) ).Cached();

		public ParameterTypeLocator( ImmutableArray<Type> types ) : base( types ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.First();
	}

	public class ResultTypeLocator : TypeLocatorCacheBase
	{
		public static ICache<Type, Type> Instance { get; } = new ResultTypeLocator( ImmutableArray.Create( typeof(IFactory<,>), typeof(IFactory<>), typeof(Func<>), typeof(Func<,>) ) ).Cached();

		public ResultTypeLocator( ImmutableArray<Type> types ) : base( types ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.Last();
	}

	public abstract class TypeLocatorCacheBase : FactoryBase<Type, Type>
	{
		readonly ImmutableArray<TypeAdapter> adapters;
		readonly Func<TypeInfo, bool> isAssignable;
		readonly Func<Type[], Type> selector;

		protected TypeLocatorCacheBase( ImmutableArray<Type> types ) : this( types.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		TypeLocatorCacheBase( ImmutableArray<TypeAdapter> adapters )
		{
			this.adapters = adapters;
			isAssignable = IsAssignable;
			selector = Select;
		}

		public override Type Create( Type parameter )
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

	/*public static class Factory
	{
		readonly static TypeAdapter[]
			CoreTypes = new[] { typeof(Func<>), typeof(Func<,>) }.Select( type => type.Adapt() ).ToArray(),
			Types = new[] { typeof(IFactory<>), typeof(IFactory<,>) }.Select( type => type.Adapt() ).ToArray();
			// BasicTypes = new[] { typeof(IFactory), typeof(IFactoryWithParameter) }.Select( type => type.Adapt() ).ToArray();
		
		/*[Freeze]
		public static bool IsFactory( [Required] Type type ) => BasicTypes.Any( adapter => adapter.IsAssignableFrom( type ) );#1#

		[Freeze]
		public static Type GetParameterType( [Required]Type factoryType ) => Get( factoryType, types => types.First(), Types.Last(), CoreTypes.Last() );

		/*[Freeze]
		public static Type GetInterface( [Required] Type factoryType ) => factoryType.Adapt().GetAllInterfaces().With( types => types.FirstOrDefault( IsGenericFactorySpecification.Instance.IsSatisfiedBy ) ?? types.FirstOrDefault( IsFactorySpecification.Instance.IsSatisfiedBy ) );#1#

		[Freeze]
		public static Type GetResultType( [Required]Type factoryType ) => Get( factoryType, types => types.Last(), Types.Concat( CoreTypes ).ToArray() );

		static Type Get( Type factoryType, Func<Type[], Type> selector, params TypeAdapter[] typesToCheck )
		{
			var result = factoryType.Adapt().GetAllInterfaces()
				.AsTypeInfos()
				.Where( type => type.IsGenericType && typesToCheck.Any( extension => extension.IsAssignableFrom( type.GetGenericTypeDefinition() ) ) )
				.Select( type => selector( type.GenericTypeArguments ) )
				.FirstOrDefault();
			return result;
		}
	}*/

	/*public class Converter<TFrom, TTo> : FactoryBase<TFrom, TTo>
	{
		public Converter( Func<TFrom, TTo> convert ) : base( convert ) {}

		// public TTo Convert( TFrom from ) => Create( from );

		// public TFrom Convert( TTo to ) => Create( to );
	}*/

	/*public interface IConverter<TFrom, TTo>
	{
		TTo Convert( TFrom parameter );

		TFrom Convert( TTo parameter );
	}

	public abstract class Converter<TFrom, TTo> : IConverter<TFrom, TTo>
	{
		readonly Func<TFrom, TTo> to;
		readonly Func<TTo, TFrom> @from;

		protected Converter( Func<TFrom, TTo> to, Func<TTo, TFrom> from )
		{
			this.to = to;
			this.from = from;
		}

		public TTo Convert( TFrom parameter ) => to( parameter );

		public TFrom Convert( TTo parameter ) => from( parameter );
	}*/

	
	public class ProjectedFactory<TFrom, TTo> : ProjectedFactory<object, TFrom, TTo>
	{
		public ProjectedFactory( Func<TFrom, TTo> convert ) : base( convert ) {}
	}

	public class ProjectedFactory<TBase, TFrom, TTo> : FactoryBase<TBase, TTo>/*, IConverter<TFrom, TTo>*/ where TFrom : TBase
	{
		readonly static ISpecification<TBase> Specification = new DelegatedSpecification<TBase>( parameter => parameter is TFrom );

		readonly Func<TFrom, TTo> convert;

		public ProjectedFactory( Func<TFrom, TTo> convert ) : base( Specification )
		{
			this.convert = convert;
		}

		public override TTo Create( TBase parameter ) => convert( (TFrom)parameter );
	}

	public class InstanceFromFactoryTypeFactory : FactoryBase<Type, object>
	{
		readonly FactoryDelegateLocatorFactory factory;

		public InstanceFromFactoryTypeFactory( [Required]FactoryDelegateLocatorFactory factory )
		{
			this.factory = factory;
		}

		public override object Create( Type parameter )
		{
			var @delegate = factory.Create( parameter );
			var result = @delegate.With( d => d() );
			return result;
		}
	}

	public class FactoryDelegateLocatorFactory : FirstFromParameterFactory<Type, Func<object>>
	{
		public FactoryDelegateLocatorFactory( FactoryDelegateFactory factory, FactoryWithActivatedParameterDelegateFactory factoryWithParameter ) : base( 
			new Factory<IFactory>( factory ),
			new Factory<IFactoryWithParameter>( factoryWithParameter )
		) {}

		[AutoValidation.GenericFactory]
		class Factory<T> : DelegatedFactory<Type, Func<object>>
		{
			public Factory( IFactory<Type, Func<object>> inner ) : base( inner.ToDelegate(), TypeAssignableSpecification<T>.Instance ) {}
		}
	}

	public sealed class MemberInfoFactoryTypeLocator : FactoryTypeLocatorBase<MemberInfo>
	{
		public MemberInfoFactoryTypeLocator( FactoryTypeLocator locator ) : base( locator, member => member.GetMemberType(), member => member.DeclaringType ) {}
	}

	public sealed class ParameterInfoFactoryTypeLocator : FactoryTypeLocatorBase<ParameterInfo>
	{
		public ParameterInfoFactoryTypeLocator( FactoryTypeLocator locator ) : base( locator, parameter => parameter.ParameterType, parameter => parameter.Member.DeclaringType ) {}
	}

	[Persistent]
	public class FactoryTypeLocator : EqualityCache<LocateTypeRequest, Type>
	{
		public FactoryTypeLocator( FactoryTypeRequest[] factoryTypes ) : base( new Factory( factoryTypes ).Create ) {}

		class Factory :  FactoryBase<LocateTypeRequest, Type>
		{
			readonly FactoryTypeRequest[] types;

			public Factory( FactoryTypeRequest[] types )
			{
				this.types = types;
			}

			public override Type Create( LocateTypeRequest parameter )
			{
				var candidates = types.Introduce( parameter, tuple => tuple.Item1.Name == tuple.Item2.Name && tuple.Item1.ResultType.Adapt().IsAssignableFrom( tuple.Item2.RequestedType ) ).ToArray();
				var item = 
					candidates.Introduce( $"{parameter.RequestedType.Name}Factory", info => info.Item1.RequestedType.Name == info.Item2 ).Only()
					??
					candidates.Introduce( parameter, arg => arg.Item1.ResultType == arg.Item2.RequestedType ).FirstOrDefault()
					??
					candidates.FirstOrDefault();

				var result = item?.RequestedType;
				return result;
			}
		}
	}
}