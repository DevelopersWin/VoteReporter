using DragonSpark.Aspects.Validation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
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
		public static ICache<Type, bool> Instance { get; } = new IsFactorySpecification( typeof(IFactory), typeof(IFactoryWithParameter) ).Cached();
		protected IsFactorySpecification( params Type[] types ) : base( types ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Adapters.IsAssignableFrom( parameter );
	}

	public class IsGenericFactorySpecification : AdapterSpecificationBase
	{
		public static ICache<Type, bool> Instance { get; } = new IsGenericFactorySpecification( typeof(IFactory<>), typeof(IFactory<,>) ).Cached();

		protected IsGenericFactorySpecification( params Type[] types ) : base( types ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Adapters.Select( adapter => adapter.Type ).Any( parameter.Adapt().IsGenericOf );
	}

	public abstract class AdapterSpecificationBase : SpecificationBase<Type>
	{
		protected AdapterSpecificationBase( params Type[] types ) : this( types.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		protected AdapterSpecificationBase( ImmutableArray<TypeAdapter> adapters )
		{
			Adapters = adapters;
		}

		protected ImmutableArray<TypeAdapter> Adapters { get; }
	}

	public class FactoryInterfaceLocator : FactoryBase<Type, Type>
	{
		public static ICache<Type, Type> Instance { get; } = new FactoryInterfaceLocator().Cached();

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

	public class InstanceFromFactoryTypeFactory : FactoryBase<Type, object>
	{
		public static InstanceFromFactoryTypeFactory Instance { get; } = new InstanceFromFactoryTypeFactory();
		InstanceFromFactoryTypeFactory() : this( FactoryDelegateLocatorFactory.Instance.Create ) {}

		readonly Func<Type, Func<object>> factory;

		InstanceFromFactoryTypeFactory( Func<Type, Func<object>> factory )
		{
			this.factory = factory;
		}

		public override object Create( Type parameter ) => factory( parameter )?.Invoke();
	}

	public class FactoryDelegateLocatorFactory : CompositeFactory<Type, Func<object>>
	{
		public static FactoryDelegateLocatorFactory Instance { get; } = new FactoryDelegateLocatorFactory();
		FactoryDelegateLocatorFactory() : base( FactoryDelegateFactory.Instance, FactoryWithActivatedParameterDelegateFactory.Instance ) {}

		public FactoryDelegateLocatorFactory( FactoryDelegateFactory factory, FactoryWithActivatedParameterDelegateFactory factoryWithParameter ) : base( 
			new Factory<IFactory>( factory ),
			new Factory<IFactoryWithParameter>( factoryWithParameter )
		) {}

		[ApplyAutoValidation]
		sealed class Factory<T> : DelegatedFactory<Type, Func<object>>
		{
			public Factory( IFactory<Type, Func<object>> inner ) : base( inner.ToDelegate(), TypeAssignableSpecification<T>.Instance ) {}
		}
	}

	public sealed class MemberInfoFactoryTypeLocator : ParameterizedConfiguration<MemberInfo, Type>
	{
		public static MemberInfoFactoryTypeLocator Instance { get; } = new MemberInfoFactoryTypeLocator();
		MemberInfoFactoryTypeLocator() : base( new FactoryTypeLocator<MemberInfo>( member => member.GetMemberType(), member => member.DeclaringType ).Create ) {}
	}

	public sealed class ParameterInfoFactoryTypeLocator : ParameterizedConfiguration<ParameterInfo, Type>
	{
		public static ParameterInfoFactoryTypeLocator Instance { get; } = new ParameterInfoFactoryTypeLocator();
		ParameterInfoFactoryTypeLocator() : base( new FactoryTypeLocator<ParameterInfo>( parameter => parameter.ParameterType, parameter => parameter.Member.DeclaringType ).Create ) {}
	}

	[Persistent]
	public class FactoryTypes : EqualityReferenceCache<LocateTypeRequest, Type>
	{
		public static IConfiguration<FactoryTypes> Instance { get; } = new Configuration<FactoryTypes>( () => new FactoryTypes( FactoryTypeRequests.Requests.Get() ) );

		public FactoryTypes( ImmutableArray<FactoryTypeRequest> requests ) : base( new Factory( requests ).Create ) {}

		class Factory :  FactoryBase<LocateTypeRequest, Type>
		{
			readonly ImmutableArray<FactoryTypeRequest> types;

			public Factory( ImmutableArray<FactoryTypeRequest> types )
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