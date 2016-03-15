using AutoMapper.Internal;
using DragonSpark.Aspects;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation.FactoryModel
{
	public static class Factory
	{
		readonly static TypeAdapter[]
			CoreTypes = new[] { typeof(Func<>), typeof(Func<,>) }.Select( type => type.Adapt() ).ToArray(),
			Types = new[] { typeof(IFactory<>), typeof(IFactory<,>) }.Select( type => type.Adapt() ).ToArray(),
			BasicTypes = new[] { typeof(IFactory), typeof(IFactoryWithParameter) }.Select( type => type.Adapt() ).ToArray();
		
		[Freeze]
		public static bool IsFactory( [Required] Type type ) => BasicTypes.Any( adapter => adapter.IsAssignableFrom( type ) );

		public static T Create<T>() => (T)Create( typeof(T) );

		public static object Create( Type type ) => FrameworkFactoryTypeLocator.Instance.Create( type ).With( From );

		public static object From( [Required, OfFactoryType]Type factoryType )
		{
			var @delegate = FactoryDelegateLocatorFactory.Instance.Create( factoryType );
			var result = @delegate.With( d => d() );
			return result;
		}

		[Freeze]
		public static Type GetParameterType( [Required]Type factoryType ) => Get( factoryType, types => types.First(), Types.Last(), CoreTypes.Last() );

		[Freeze]
		public static Type GetInterface( [Required] Type factoryType ) => factoryType.Adapt().GetAllInterfaces().FirstOrDefault( IsFactory );

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

	public class FactoryDelegateLocatorFactory : FirstFromParameterFactory<Type, Func<object>>
	{
		public static FactoryDelegateLocatorFactory Instance { get; } = new FactoryDelegateLocatorFactory( FactoryDelegateFactory.Instance, FactoryWithActivatedParameterDelegateFactory.Instance );

		public FactoryDelegateLocatorFactory( FactoryDelegateFactory factory, FactoryWithActivatedParameterDelegateFactory factoryWithParameter ) : base( 
			new Factory<IFactory>( factory ),
			new Factory<IFactoryWithParameter>( factoryWithParameter )
		) {}

		class Factory<T> : DecoratedFactory<Type, Func<object>>
		{
			public Factory( IFactory<Type, Func<object>> inner ) : base( TypeAssignableSpecification<T>.Instance, inner.Create ) {}
		}
	}

	public class MemberInfoFactoryTypeLocator : FactoryTypeLocatorBase<MemberInfo>
	{
		public static MemberInfoFactoryTypeLocator Instance { get; } = new MemberInfoFactoryTypeLocator();

		public MemberInfoFactoryTypeLocator() : base( member => member.GetMemberType(), member => new[] { member.DeclaringType } ) {}
	}

	public class ParameterInfoFactoryTypeLocator : FactoryTypeLocatorBase<ParameterInfo>
	{
		public static ParameterInfoFactoryTypeLocator Instance { get; } = new ParameterInfoFactoryTypeLocator();

		public ParameterInfoFactoryTypeLocator() : base( parameter => parameter.ParameterType, parameter => new[] { parameter.Member.DeclaringType } ) {}
	}

	public class FrameworkFactoryTypeLocator : FactoryTypeLocatorBase<Type>
	{
		public static FrameworkFactoryTypeLocator Instance { get; } = new FrameworkFactoryTypeLocator( Assemblies.GetCurrent );

		public FrameworkFactoryTypeLocator( Assemblies.Get assemblies ) : base( assemblies, Default<Type>.Self, t => Default<Type>.Items ) {}
	}

	public abstract class FactoryTypeLocatorBase<T> : FactoryBase<T, Type>
	{
		readonly Assemblies.Get assemblies;
		readonly Func<T, Type> type;
		readonly Func<T, Type[]> locations;

		protected FactoryTypeLocatorBase( [Required]Func<T, Type> type, [Required]Func<T, Type[]> locations ) : this( Assemblies.GetCurrent, type, locations ) {}

		protected FactoryTypeLocatorBase( Assemblies.Get assemblies, [Required]Func<T, Type> type, [Required]Func<T, Type[]> locations )
		{
			this.assemblies = assemblies;
			this.type = type;
			this.locations = locations;
		}

		[Freeze]
		protected override Type CreateItem( T parameter )
		{
			var mapped = new CompositionContract( type( parameter ) );
			var candidates = new[] { locations( parameter ).Append( mapped.ContractType, GetType() ).Assemblies().Distinct().Fixed, assemblies };
			var result = candidates.Select( get => new DiscoverableFactoryTypeLocator( new FactoryTypeContainer( get.GetTypes() ) ) ).FirstWhere( get => get.Create( mapped ) );
			return result;
		}
	}

	[Persistent]
	public class DiscoverableFactoryTypeLocator : FactoryBase<CompositionContract, Type>
	{
		readonly FactoryTypeContainer container;

		public DiscoverableFactoryTypeLocator( [Required]FactoryTypeContainer container )
		{
			this.container = container;
		}

		[Freeze]
		protected override Type CreateItem( CompositionContract parameter )
		{
			var name = $"{parameter.ContractType.Name}Factory";
			var candidates = container.Where( type => parameter.ContractName == type.Name && type.ResultType.Adapt().IsAssignableFrom( parameter.ContractType ) ).ToArray();
			var item = 
				candidates.Only( info => info.FactoryType.Name == name )
				??
				candidates.FirstOrDefault( arg => arg.ResultType == parameter.ContractType )
				??
				candidates.FirstOrDefault();
			var result = item.With( profile => profile.FactoryType );
			return result;
		}
	}
}