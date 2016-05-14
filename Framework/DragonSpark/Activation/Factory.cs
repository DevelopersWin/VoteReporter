using DragonSpark.Aspects;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation
{
	public static class Factory
	{
		readonly static TypeAdapter[]
			CoreTypes = new[] { typeof(Func<>), typeof(Func<,>) }.Select( type => type.Adapt() ).ToArray(),
			Types = new[] { typeof(IFactory<>), typeof(IFactory<,>) }.Select( type => type.Adapt() ).ToArray(),
			BasicTypes = new[] { typeof(IFactory), typeof(IFactoryWithParameter) }.Select( type => type.Adapt() ).ToArray();
		
		[Freeze]
		public static bool IsFactory( [Required] Type type ) => BasicTypes.Any( adapter => adapter.IsAssignableFrom( type ) );

		[Freeze]
		public static bool IsGenericFactory( [Required] Type type ) => Types.Any( adapter => type.Adapt().IsGenericOf( adapter.Type ) );

		[Freeze]
		public static Type GetParameterType( [Required]Type factoryType ) => Get( factoryType, types => types.First(), Types.Last(), CoreTypes.Last() );

		[Freeze]
		public static Type GetInterface( [Required] Type factoryType ) => factoryType.Adapt().GetAllInterfaces().With( types => types.FirstOrDefault( IsGenericFactory ) ?? types.FirstOrDefault( IsFactory ) );

		[Freeze]
		public static Type GetResultType( [Required]Type factoryType ) => Get( factoryType, types => types.Last(), Types.Concat( CoreTypes ).ToArray() );

		static Type Get( Type factoryType, Func<Type[], Type> selector, params TypeAdapter[] typesToCheck )
		{
			var result = factoryType.Append( factoryType.Adapt().GetAllInterfaces() )
				.AsTypeInfos()
				.Where( type => type.IsGenericType && typesToCheck.Any( extension => extension.IsAssignableFrom( type.GetGenericTypeDefinition() ) ) )
				.Select( type => selector( type.GenericTypeArguments ) )
				.FirstOrDefault();
			return result;
		}
	}

	public class Converter<TFrom, TTo> : Converter<object, TFrom, TTo>
	{
		public Converter( Func<TFrom, TTo> convert ) : base( convert ) {}
	}

	public class Converter<TBase, TFrom, TTo> : FactoryBase<TBase, TTo> where TFrom : TBase
	{
		readonly Func<TFrom, TTo> convert;

		public Converter( Func<TFrom, TTo> convert )
		{
			this.convert = convert;
		}

		public override TTo Create( TBase parameter ) => parameter.AsTo<TFrom, TTo>( @from => convert( @from ) );
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
		public MemberInfoFactoryTypeLocator( FactoryTypeRequestLocator locator ) : base( locator, member => member.GetMemberType(), member => member.DeclaringType ) {}
	}

	// [Export]
	public sealed class ParameterInfoFactoryTypeLocator : FactoryTypeLocatorBase<ParameterInfo>
	{
		// [ImportingConstructor]
		public ParameterInfoFactoryTypeLocator( FactoryTypeRequestLocator locator ) : base( locator, parameter => parameter.ParameterType, parameter => parameter.Member.DeclaringType ) {}
	}

	public abstract class FactoryTypeLocatorBase<T> : FactoryBase<T, Type>
	{
		readonly FactoryTypeRequestLocator locator;
		readonly Func<T, Type> type;
		readonly Func<T, Type> context;

		protected FactoryTypeLocatorBase( [Required]FactoryTypeRequestLocator locator, [Required]Func<T, Type> type, [Required]Func<T, Type> context )
		{
			this.locator = locator;
			this.type = type;
			this.context = context;
		}

		[Freeze]
		public override Type Create( T parameter )
		{
			var info = context( parameter ).GetTypeInfo();
			var nestedTypes = info.DeclaredNestedTypes.ToArray();
			var all = nestedTypes.Concat( info.Assembly.DefinedTypes.Except( nestedTypes ) );
			var location = FactoryTypeFactory.Instance.CreateMany( all.AsTypes() );
			var mapped = new LocateTypeRequest( type( parameter ) );
			var locators = new[] { new FactoryTypeRequestLocator( location ), locator };
			var result = locators.FirstWhere( typeLocator => typeLocator.Create( mapped ) );
			return result;
		}
	}

	[Persistent]
	public class FactoryTypeRequestLocator : FactoryBase<LocateTypeRequest, Type>
	{
		/*public static FactoryTypeRequestLocator Instance { get; } = new FactoryTypeRequestLocator( Default<FactoryTypeRequest>.Items );*/

		readonly FactoryTypeRequest[] types;

		public FactoryTypeRequestLocator( [Required] FactoryTypeRequest[] types )
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