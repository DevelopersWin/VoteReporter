﻿using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Setup.Registration;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DragonSpark.Specifications;
using Defaults = DragonSpark.Sources.Parameterized.Defaults;

namespace DragonSpark.Composition
{
	[ApplyAutoValidation]
	public sealed class ConventionTypes : ValidatedParameterizedSourceBase<Type, Type>
	{
		readonly static ISpecification<Type> Specification = InstantiableTypeSpecification.Default.And( CanInstantiateSpecification.Default.Inverse() );
		readonly static Func<Type, bool> Activate = Defaults.ActivateSpecification.IsSatisfiedBy;

		public static IParameterizedSource<Type, Type> Default { get; } = new ParameterizedScope<Type, Type>( new ConventionTypes().ToSourceDelegate().Global() );
		ConventionTypes() : this( ApplicationTypes.Default ) {}

		readonly ISource<ImmutableArray<Type>> source;

		public ConventionTypes( ISource<ImmutableArray<Type>> source ) : base( Specification )
		{
			this.source = source;
		}

		static Type Map( Type parameter )
		{
			var name = $"{parameter.Namespace}.{ConventionCandidateNames.Default.Get( parameter )}";
			var result = name != parameter.FullName ? parameter.Assembly().GetType( name ) : null;
			return result;
		}

		Type Search( Type parameter )
		{
			var adapter = parameter.Adapt();
			var convention = IsConventionCandidateSpecification.Defaults.Get( parameter );
			var result =
					source.Get()
					.Where( adapter.IsAssignableFrom )
					.Where( Activate )
					.FirstOrDefault( convention );
			return result;
		}

		public override Type Get( Type parameter ) => Map( parameter ) ?? Search( parameter );
	}

	class IsConventionCandidateSpecification : SpecificationBase<Type>
	{
		public static IParameterizedSource<Type, Func<Type, bool>> Defaults { get; } = new Cache<Type, Func<Type, bool>>( t => new IsConventionCandidateSpecification( ConventionCandidateNames.Default.Get( t ) ).IsSatisfiedBy );
		
		readonly string name;

		public IsConventionCandidateSpecification( string name )
		{
			this.name = name;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter.Name.Equals( name );
	}

	sealed class ConventionCandidateNames : Cache<Type, string>
	{
		public static ConventionCandidateNames Default { get; } = new ConventionCandidateNames();
		ConventionCandidateNames() : base( type => type.Name.TrimStartOf( 'I' ) ) {}
	}

	public sealed class SelfAndNestedTypes : Cache<Type, IEnumerable<Type>>
	{
		public static SelfAndNestedTypes Default { get; } = new SelfAndNestedTypes();
		SelfAndNestedTypes() : base( type => type.Adapt().WithNested() ) {}
	}

	public sealed class ConventionMappings : ParameterizedSourceBase<Type, ConventionMapping>
	{
		public static IParameterizedSource<Type, ConventionMapping> Default { get; } = new ConventionMappings().ToCache();
		ConventionMappings() {}

		public override ConventionMapping Get( Type parameter )
		{
			var @interface = ConventionImplementedInterfaces.Default.Get( parameter );
			var result = @interface != null ? new ConventionMapping( @interface, parameter ) : default(ConventionMapping);
			return result;
		}
	}

	public struct ConventionMapping
	{
		public ConventionMapping( Type interfaceType, Type implementationType )
		{
			InterfaceType = interfaceType;
			ImplementationType = implementationType;
		}

		public Type InterfaceType { get; }
		public Type ImplementationType { get; }
	}

	public sealed class ConventionImplementedInterfaces : FactoryCache<Type, Type>
	{
		public static ConventionImplementedInterfaces Default { get; } = new ConventionImplementedInterfaces();
		ConventionImplementedInterfaces() : this( typeof(ISource), typeof(IParameterizedSource), typeof(IValidatedParameterizedSource) ) {}

		readonly ImmutableArray<Type> exempt;

		public ConventionImplementedInterfaces( params Type[] exempt )
		{
			this.exempt = exempt.ToImmutableArray();
		}

		protected override Type Create( Type parameter )
		{
			foreach ( var @interface in parameter.GetTypeInfo().ImplementedInterfaces.Except( exempt.ToArray() ).ToArray() )
			{
				var specification = IsConventionCandidateSpecification.Defaults.Get( @interface );
				if ( specification( parameter ) )
				{
					return @interface;
				}
			}
			return null;
		}
	}

	public sealed class CanInstantiateSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new CanInstantiateSpecification().ToCachedSpecification();
		CanInstantiateSpecification() {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = !info.IsGenericTypeDefinition && !info.ContainsGenericParameters && !info.IsInterface && !info.IsAbstract && info.DeclaredConstructors.Any( constructorInfo => constructorInfo.IsPublic ) && ( info.IsPublic || info.Assembly.Has<RegistrationAttribute>() );
			return result;
		}
	}

	public class InstantiableTypeSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new InstantiableTypeSpecification().ToCachedSpecification();
		InstantiableTypeSpecification() : this( typeof(Delegate), typeof(Array) ) {}

		readonly ImmutableArray<TypeAdapter> exempt;

		public InstantiableTypeSpecification( params Type[] exempt )
		{
			this.exempt = exempt.Select( type => type.Adapt() ).ToImmutableArray();
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter != typeof(object) && !exempt.IsAssignableFrom( parameter );
	}
}
