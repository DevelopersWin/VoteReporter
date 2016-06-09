using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation
{
	public class IsFactorySpecification : AdapterSpecificationBase
	{
		public static IsFactorySpecification Instance { get; } = new IsFactorySpecification( typeof(IFactory), typeof(IFactoryWithParameter) );

		public IsFactorySpecification( params Type[] types ) : base( types ) {}

		[Freeze]
		public override bool IsSatisfiedBy( Type parameter ) => Adapters.Any( adapter => adapter.IsAssignableFrom( parameter ) );
	}

	public class IsGenericFactorySpecification : AdapterSpecificationBase
	{
		public static IsGenericFactorySpecification Instance { get; } = new IsGenericFactorySpecification( typeof(IFactory<>), typeof(IFactory<,>) );

		public IsGenericFactorySpecification( params Type[] types ) : base( types ) {}

		[Freeze]
		public override bool IsSatisfiedBy( Type parameter )
		{
			var typeAdapter = parameter.Adapt();
			var result = Adapters.Any( adapter => typeAdapter.IsGenericOf( adapter.Type ) );
			return result;
		}
	}

	public abstract class AdapterSpecificationBase : SpecificationBase<Type>
	{
		protected AdapterSpecificationBase( params Type[] types ) : this( EnumerableExtensions.Fixed( types.Select( type => type.Adapt() ) ) ) {}

		protected AdapterSpecificationBase( params TypeAdapter[] adapters )
		{
			Adapters = adapters;
		}

		protected TypeAdapter[] Adapters { get; }
	}

	// [AutoValidation( false )]
	public class FactoryInterfaceLocator : FactoryBase<Type, Type>
	{
		public static FactoryInterfaceLocator Instance { get; } = new FactoryInterfaceLocator();

		[Freeze]
		public override Type Create( Type parameter ) => parameter.Adapt().GetAllInterfaces().With( types => types.FirstOrDefault( IsGenericFactorySpecification.Instance.IsSatisfiedBy ) ?? types.FirstOrDefault( IsFactorySpecification.Instance.IsSatisfiedBy ) );
	}

	public class ParameterTypeLocator : TypeLocatorBase
	{
		public static ParameterTypeLocator Instance { get; } = new ParameterTypeLocator( typeof(Func<,>), typeof(IFactory<,>), typeof(ICommand<>) );

		public ParameterTypeLocator( params Type[] types ) : base( types ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.First();
	}

	public class ResultTypeLocator : TypeLocatorBase
	{
		public static ResultTypeLocator Instance { get; } = new ResultTypeLocator( typeof(IFactory<,>), typeof(IFactory<>), typeof(Func<>), typeof(Func<,>) );

		public ResultTypeLocator( params Type[] types ) : base( types ) {}

		protected override Type Select( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.Last();
	}

	public abstract class TypeLocatorBase : FactoryBase<Type, Type>
	{
		readonly TypeAdapter[] adapters;

		protected TypeLocatorBase( params Type[] types ) : this( EnumerableExtensions.Fixed( types.Select( type => type.Adapt() ) ) ) {}

		TypeLocatorBase( params TypeAdapter[] adapters )
		{
			this.adapters = adapters;
		}

		[Freeze]
		public override Type Create( Type parameter )
		{
			var result = parameter.Append( parameter.Adapt().GetAllInterfaces() )
				.AsTypeInfos()
				.Where( type => type.IsGenericType && type.GetGenericTypeDefinition().With( definition => adapters.Any( adapter => adapter.IsAssignableFrom( definition ) ) ) )
				.Select( type => Select( type.GenericTypeArguments ) )
				.FirstOrDefault();
			return result;
		}

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

	public abstract class Converter<TBase, TFrom, TTo> : FactoryBase<TBase, TTo>/*, IConverter<TFrom, TTo>*/ where TFrom : TBase
	{
		readonly Func<TFrom, TTo> convert;

		protected Converter( Func<TFrom, TTo> convert )
		{
			this.convert = convert;
		}

		public override TTo Create( TBase parameter ) => parameter.AsTo<TFrom, TTo>( Convert );

		public TTo Convert( TFrom parameter ) => convert( parameter );

		/*TFrom IConverter<TFrom, TTo>.Convert( TTo parameter )
		{
			throw new NotSupportedException();
		}*/
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

		class Factory<T> : DelegatedFactory<Type, Func<object>>
		{
			public Factory( IFactory<Type, Func<object>> inner ) : base( TypeAssignableSpecification<T>.Instance, inner.Create ) {}
		}
	}

	// [Export]
	public sealed class MemberInfoFactoryTypeLocator : FactoryTypeLocatorBase<MemberInfo>
	{
		// [ImportingConstructor]
		public MemberInfoFactoryTypeLocator( FactoryTypeLocator locator ) : base( locator, member => member.GetMemberType(), member => member.DeclaringType ) {}
	}

	// [Export]
	public sealed class ParameterInfoFactoryTypeLocator : FactoryTypeLocatorBase<ParameterInfo>
	{
		// [ImportingConstructor]
		public ParameterInfoFactoryTypeLocator( FactoryTypeLocator locator ) : base( locator, parameter => parameter.ParameterType, parameter => parameter.Member.DeclaringType ) {}
	}

	[Persistent]
	public class FactoryTypeLocator : FactoryBase<LocateTypeRequest, Type>
	{
		/*public static FactoryTypeLocator Instance { get; } = new FactoryTypeLocator( Default<FactoryTypeRequest>.Items );*/

		readonly FactoryTypeRequest[] types;

		public FactoryTypeLocator( [Required] FactoryTypeRequest[] types )
		{
			this.types = types;
		}

		[Freeze]
		public override Type Create( LocateTypeRequest parameter )
		{
			var name = $"{parameter.RequestedType.Name}Factory";
			var candidates = types.Where( type => parameter.Name == type.Name && type.ResultType.Adapt().IsAssignableFrom( parameter.RequestedType ) ).ToArray();
			var item = 
				candidates.Only( info => info.RequestedType.Name == name )
				??
				candidates.FirstOrDefault( arg => arg.ResultType == parameter.RequestedType )
				??
				candidates.FirstOrDefault();
			var result = item.With( profile => profile.RequestedType );
			return result;
		}
	}
}