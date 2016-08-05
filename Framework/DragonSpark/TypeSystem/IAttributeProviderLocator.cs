using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class TypeDefinitionProviderStore : ItemsStoreBase<ITypeDefinitionProvider>
	{
		public static TypeDefinitionProviderStore Instance { get; } = new TypeDefinitionProviderStore();
		TypeDefinitionProviderStore() : this( Items<ITypeDefinitionProvider>.Default ) {}

		protected TypeDefinitionProviderStore( params ITypeDefinitionProvider[] items ) : base( items ) {}

		protected override IEnumerable<ITypeDefinitionProvider> Yield() => base.Yield().Append( ConventionTypeDefinitionProvider.Instance, Self.Instance );

		class Self : SelfTransformer<TypeInfo>, ITypeDefinitionProvider
		{
			public new static Self Instance { get; } = new Self();
			Self() {}
		}
	}

	public sealed class AttributeProviders : Cache<IAttributeProvider>
	{
		readonly static Func<object, IAttributeProvider> Delegate = Factory.Instance.Create;

		public static ISource<ICache<IAttributeProvider>> Instance { get; } = new ExecutionScope<ICache<IAttributeProvider>>( () => new AttributeProviders() );
		AttributeProviders() : base( Delegate ) {}

		sealed class Factory : ParameterConstructedCompositeFactory<IAttributeProvider>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() : this( MemberInfoDefinitionLocator.Instance.Delegate(), ReflectionElementAttributeProvider.Default.ToDelegate() ) {}

			readonly Func<object, MemberInfo> memberSource;
			readonly Func<object, IAttributeProvider> providerSource;
			
			Factory( Func<object, MemberInfo> memberSource, Func<object, IAttributeProvider> providerSource ) : base( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider) )
			{
				this.memberSource = memberSource;
				this.providerSource = providerSource;
			}

			public override IAttributeProvider Create( object parameter ) => base.Create( parameter ) ?? providerSource( memberSource( parameter ) );
		}
	}

	class ReflectionElementAttributeProvider : ParameterConstructedCompositeFactory<IAttributeProvider>
	{
		public static ICache<IAttributeProvider> Default { get; } = new ReflectionElementAttributeProvider().Cached();
		ReflectionElementAttributeProvider() : base( typeof(TypeInfoAttributeProvider), typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(MemberInfoAttributeProvider) ) {}
	}

	class TypeDefinitionLocator : CompositeFactory<object, TypeInfo>
	{
		readonly static Func<object, TypeInfo>[] Factories = new IFactoryWithParameter[] { TypeInfoDefinitionProvider.Instance, MemberInfoDefinitionProvider.Instance, GeneralDefinitionProvider.Instance }.Select( parameter => new Func<object, TypeInfo>( parameter.Create<TypeInfo> ) ).Fixed();
		readonly static Func<TypeInfo, TypeInfo> Delegate = TypeDefinitionProvider.Instance.Delegate();

		public static ISource<ICache<TypeInfo>> Instance { get; } = new ExecutionScope<ICache<TypeInfo>>( () => new TypeDefinitionLocator().Cached() );
		TypeDefinitionLocator() : this( Delegate ) { }

		readonly Func<TypeInfo, TypeInfo> source;
		
		TypeDefinitionLocator( Func<TypeInfo, TypeInfo> source ) : base( Factories )
		{
			this.source = source;
		}

		class TypeInfoDefinitionProvider : TypeDefinitionProviderBase<TypeInfo>
		{
			public static TypeInfoDefinitionProvider Instance { get; } = new TypeInfoDefinitionProvider();

			public override TypeInfo Create( TypeInfo parameter ) => parameter;
		}

		class MemberInfoDefinitionProvider : TypeDefinitionProviderBase<MemberInfo>
		{
			public static MemberInfoDefinitionProvider Instance { get; } = new MemberInfoDefinitionProvider();

			public override TypeInfo Create( MemberInfo parameter ) => parameter.DeclaringType.GetTypeInfo();
		}

		class GeneralDefinitionProvider : TypeDefinitionProviderBase<object>
		{
			public static GeneralDefinitionProvider Instance { get; } = new GeneralDefinitionProvider();

			public override TypeInfo Create( object parameter ) => parameter.GetType().GetTypeInfo();
		}

		abstract class TypeDefinitionProviderBase<T> : FactoryBase<T, TypeInfo> {}

		public override TypeInfo Create( object parameter ) => source( base.Create( parameter ) );
	}

	class MemberInfoDefinitionLocator : FactoryBase<object, MemberInfo>
	{
		readonly static ImmutableArray<Func<object, IFactoryWithParameter>> Delegates = new[] { typeof(PropertyInfoDefinitionLocator), typeof(ConstructorInfoDefinitionLocator), typeof(MethodInfoDefinitionLocator), typeof(TypeInfoDefinitionLocator) }.Select( type => ParameterConstructor<IFactoryWithParameter>.Make( typeof(TypeInfo), type ) ).ToImmutableArray();
		readonly static Func<object, TypeInfo> TypeSource = TypeDefinitionLocator.Instance.Delegate();

		public static ISource<ICache<MemberInfo>> Instance { get; } = new ExecutionScope<ICache<MemberInfo>>( () => new MemberInfoDefinitionLocator( TypeSource ).Cached() );

		readonly Func<object, TypeInfo> typeSource;

		MemberInfoDefinitionLocator( Func<object, TypeInfo> typeSource )
		{
			this.typeSource = typeSource;
		}

		class PropertyInfoDefinitionLocator : NamedMemberInfoDefinitionLocatorBase<PropertyInfo>
		{
			public PropertyInfoDefinitionLocator( TypeInfo definition ) : base( definition, definition.GetDeclaredProperty, definition.DeclaredProperties ) {}
		}

		class ConstructorInfoDefinitionLocator : MemberInfoDefinitionLocatorBase<ConstructorInfo>
		{
			public ConstructorInfoDefinitionLocator( TypeInfo definition ) : base( definition ) {}
			protected override MemberInfo From( ConstructorInfo parameter ) => 
				Definition.DeclaredConstructors.Introduce( parameter.GetParameterTypes(), tuple => tuple.Item1.GetParameterTypes().SequenceEqual( tuple.Item2 ) ).FirstOrDefault();
		}

		class MethodInfoDefinitionLocator : NamedMemberInfoDefinitionLocatorBase<MethodInfo>
		{
			public MethodInfoDefinitionLocator( TypeInfo definition ) : base( definition, definition.GetDeclaredMethod, definition.DeclaredMethods ) {}
		}

		class TypeInfoDefinitionLocator : MemberInfoDefinitionLocatorBase<object>
		{
			public TypeInfoDefinitionLocator( TypeInfo definition ) : base( definition ) {}

			protected override MemberInfo From( object parameter ) => Definition;
		}

		abstract class NamedMemberInfoDefinitionLocatorBase<T> : MemberInfoDefinitionLocatorBase<T> where T : MemberInfo
		{
			readonly Func<string, T> source;
			readonly IEnumerable<T> all;

			protected NamedMemberInfoDefinitionLocatorBase( TypeInfo definition, Func<string, T> source, IEnumerable<T> all ) : base( definition )
			{
				this.source = source;
				this.all = all;
			}

			protected override MemberInfo From( T parameter )
			{
				try
				{
					return source( parameter.Name );
				}
				catch ( AmbiguousMatchException )
				{
					var result = all.Introduce( parameter, tuple => tuple.Item1.Name == tuple.Item2.Name ).FirstOrDefault();
					return result;
				}
			}
		}

		abstract class MemberInfoDefinitionLocatorBase<T> : FactoryBase<T, MemberInfo>
		{
			protected MemberInfoDefinitionLocatorBase( TypeInfo definition )
			{
				Definition = definition;
			}

			public override MemberInfo Create( T parameter ) => From( parameter ) ?? parameter as MemberInfo ?? Definition;

			protected abstract MemberInfo From( T parameter );

			protected TypeInfo Definition { get; }
		}

		public override MemberInfo Create( object parameter )
		{
			var definition = typeSource( parameter );
			foreach ( var @delegate in Delegates )
			{
				var factory = @delegate( definition );
				if ( factory?.CanCreate( parameter ) ?? false )
				{
					return factory.Create<MemberInfo>( parameter );
				}
			}
			return null;
		}
	}
}