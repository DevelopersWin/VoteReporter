using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem.Metadata
{
	sealed class MemberInfoDefinitions : ParameterizedScope<MemberInfo>
	{
		public static IParameterizedSource<MemberInfo> Default { get; } = new MemberInfoDefinitions();
		MemberInfoDefinitions() : base( new Factory( TypeDefinitions.Default.Get ).ToSourceDelegate().Global() ) {}

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
					if ( factory?.IsSatisfiedBy( parameter ) ?? false )
					{
						return factory.Get<MemberInfo>( parameter );
					}
				}
				return null;
			}
		}
	}
}