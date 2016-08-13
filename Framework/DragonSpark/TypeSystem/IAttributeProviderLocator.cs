using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
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

		sealed class Self : SelfTransformer<TypeInfo>, ITypeDefinitionProvider
		{
			public new static Self Instance { get; } = new Self();
			Self() {}
		}
	}

	public sealed class AttributeProviders : ParameterizedScope<IAttributeProvider>
	{
		public static IParameterizedSource<IAttributeProvider> Instance { get; } = new AttributeProviders();
		AttributeProviders() : base( new Factory().ToSourceDelegate().CachedPerScope() ) {}

		sealed class Factory : ParameterConstructedCompositeFactory<IAttributeProvider>
		{
			public Factory() : this( MemberInfoDefinitions.Instance.Get, ReflectionElementAttributeProvider.Default.ToSourceDelegate() ) {}

			readonly Func<object, MemberInfo> memberSource;
			readonly Func<object, IAttributeProvider> providerSource;
			
			Factory( Func<object, MemberInfo> memberSource, Func<object, IAttributeProvider> providerSource ) : base( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider) )
			{
				this.memberSource = memberSource;
				this.providerSource = providerSource;
			}

			public override IAttributeProvider Get( object parameter ) => base.Get( parameter ) ?? providerSource( memberSource( parameter ) );
		}
	}

	sealed class ReflectionElementAttributeProvider : ParameterConstructedCompositeFactory<IAttributeProvider>
	{
		public static IParameterizedSource<IAttributeProvider> Default { get; } = new ReflectionElementAttributeProvider().ToCache();
		ReflectionElementAttributeProvider() : base( typeof(TypeInfoAttributeProvider), typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(MemberInfoAttributeProvider) ) {}
	}

	sealed class TypeDefinitions : ParameterizedScope<TypeInfo>
	{
		public static TypeDefinitions Instance { get; } = new TypeDefinitions();
		TypeDefinitions() : base( new Factory().ToSourceDelegate().CachedPerScope() ) {}

		sealed class Factory : CompositeFactory<object, TypeInfo>
		{
			readonly static Func<object, TypeInfo>[] Factories = new IParameterizedSource[] { TypeInfoDefinitionProvider.Instance, MemberInfoDefinitionProvider.Instance, GeneralDefinitionProvider.Instance }.Select( parameter => new Func<object, TypeInfo>( parameter.Get<TypeInfo> ) ).Fixed();

			public Factory() : this( ComponentModel.TypeDefinitions.Instance.Get ) { }

			readonly Func<TypeInfo, TypeInfo> source;
		
			Factory( Func<TypeInfo, TypeInfo> source ) : base( Factories )
			{
				this.source = source;
			}

			class TypeInfoDefinitionProvider : TypeDefinitionProviderBase<TypeInfo>
			{
				public static TypeInfoDefinitionProvider Instance { get; } = new TypeInfoDefinitionProvider();

				public override TypeInfo Get( TypeInfo parameter ) => parameter;
			}

			class MemberInfoDefinitionProvider : TypeDefinitionProviderBase<MemberInfo>
			{
				public static MemberInfoDefinitionProvider Instance { get; } = new MemberInfoDefinitionProvider();

				public override TypeInfo Get( MemberInfo parameter ) => parameter.DeclaringType.GetTypeInfo();
			}

			class GeneralDefinitionProvider : TypeDefinitionProviderBase<object>
			{
				public static GeneralDefinitionProvider Instance { get; } = new GeneralDefinitionProvider();

				public override TypeInfo Get( object parameter ) => parameter.GetType().GetTypeInfo();
			}

			abstract class TypeDefinitionProviderBase<T> : ParameterizedSourceBase<T, TypeInfo> {}

			public override TypeInfo Get( object parameter ) => source( base.Get( parameter ) );
		}
	}

	sealed class MemberInfoDefinitions : ParameterizedScope<MemberInfo>
	{
		public static IParameterizedSource<MemberInfo> Instance { get; } = new MemberInfoDefinitions();
		MemberInfoDefinitions() : base( new Factory( TypeDefinitions.Instance.Get ).ToSourceDelegate().CachedPerScope() ) {}

		sealed class Factory : ParameterizedSourceBase<MemberInfo>
		{
			readonly static ImmutableArray<Func<object, IValidatedParameterizedSource>> Delegates = new[] { typeof(PropertyInfoDefinitionLocator), typeof(ConstructorInfoDefinitionLocator), typeof(MethodInfoDefinitionLocator), typeof(TypeInfoDefinitionLocator) }.Select( type => ParameterConstructor<IValidatedParameterizedSource>.Make( typeof(TypeInfo), type ) ).ToImmutableArray();
		
			readonly Func<object, TypeInfo> typeSource;

			public Factory( Func<object, TypeInfo> typeSource )
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

			abstract class MemberInfoDefinitionLocatorBase<T> : ValidatedParameterizedSourceBase<T, MemberInfo>
			{
				protected MemberInfoDefinitionLocatorBase( TypeInfo definition )
				{
					Definition = definition;
				}

				public override MemberInfo Get( T parameter ) => From( parameter ) ?? parameter as MemberInfo ?? Definition;

				protected abstract MemberInfo From( T parameter );

				protected TypeInfo Definition { get; }
			}

			public override MemberInfo Get( object parameter )
			{
				var definition = typeSource( parameter );
				foreach ( var @delegate in Delegates )
				{
					var factory = @delegate( definition );
					if ( factory?.IsValid( parameter ) ?? false )
					{
						return factory.Get<MemberInfo>( parameter );
					}
				}
				return null;
			}
		}
	}
}