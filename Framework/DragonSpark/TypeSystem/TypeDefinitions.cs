using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	sealed class TypeDefinitions : ParameterizedScope<TypeInfo>
	{
		public static TypeDefinitions Default { get; } = new TypeDefinitions();
		TypeDefinitions() : base( new Factory().ToSourceDelegate().Global() ) {}

		sealed class Factory : CompositeFactory<object, TypeInfo>
		{
			readonly static Func<object, TypeInfo>[] Factories = new IParameterizedSource[] { TypeInfoDefinitionProvider.DefaultNested, MemberInfoDefinitionProvider.DefaultNested, GeneralDefinitionProvider.DefaultNested }.Select( parameter => new Func<object, TypeInfo>( parameter.Get<TypeInfo> ) ).Fixed();

			public Factory() : this( ComponentModel.TypeDefinitions.Default.Get ) { }

			readonly Func<TypeInfo, TypeInfo> source;
		
			Factory( Func<TypeInfo, TypeInfo> source ) : base( Factories )
			{
				this.source = source;
			}

			class TypeInfoDefinitionProvider : TypeDefinitionProviderBase<TypeInfo>
			{
				public static TypeInfoDefinitionProvider DefaultNested { get; } = new TypeInfoDefinitionProvider();

				public override TypeInfo Get( TypeInfo parameter ) => parameter;
			}

			class MemberInfoDefinitionProvider : TypeDefinitionProviderBase<MemberInfo>
			{
				public static MemberInfoDefinitionProvider DefaultNested { get; } = new MemberInfoDefinitionProvider();

				public override TypeInfo Get( MemberInfo parameter ) => parameter.DeclaringType.GetTypeInfo();
			}

			class GeneralDefinitionProvider : TypeDefinitionProviderBase<object>
			{
				public static GeneralDefinitionProvider DefaultNested { get; } = new GeneralDefinitionProvider();

				public override TypeInfo Get( object parameter ) => parameter.GetType().GetTypeInfo();
			}

			abstract class TypeDefinitionProviderBase<T> : ParameterizedSourceBase<T, TypeInfo> {}

			public override TypeInfo Get( object parameter = null ) => source( base.Get( parameter ) );
		}
	}
}