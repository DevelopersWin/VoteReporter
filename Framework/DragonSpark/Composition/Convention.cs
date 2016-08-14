using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.Setup.Registration;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using Defaults = DragonSpark.Sources.Parameterized.Defaults;

namespace DragonSpark.Composition
{
	public class ConventionExporter : ExportDescriptorProvider
	{
		readonly IParameterizedSource<Type, Type> candidates;

		public ConventionExporter() : this( new ConventionCandidates() ) {}

		public ConventionExporter( IParameterizedSource<Type, Type> candidates )
		{
			this.candidates = candidates;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			var exists = descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency );
			if ( !exists )
			{
				
			}
			yield break;
		}
	}

	public sealed class ConventionCandidates : FactoryCache<Type, Type>
	{
		readonly static ISpecification<Type> Specification = InstantiableTypeSpecification.Instance.And( CanInstantiateSpecification.Instance.Inverse() );

		public ConventionCandidates() : base( Specification ) {}
		protected override Type Create( Type parameter ) => ConventionTypes.Instance.Get( parameter );
	}

	public sealed class ConventionTypes : ParameterizedScope<Type, Type>
	{
		readonly static Func<Type, ITypeCandidateWeightProvider> Weight = ParameterConstructor<Type, TypeCandidateWeightProvider>.Default;
		readonly static Func<Type, bool> Specification = Defaults.ActivateSpecification.IsSatisfiedBy;

		public static IParameterizedSource<Type, Type> Instance { get; } = new ConventionTypes();
		ConventionTypes() : this( ApplicationTypes.Instance ) {}

		public ConventionTypes( ITypeSource source ) : base( new Locator( source ).ToSourceDelegate().ForGlobalScope() ) {}

		[ApplyAutoValidation]
		sealed class Locator : ValidatedParameterizedSourceBase<Type, Type>
		{
			readonly ITypeSource source;

			public Locator( ITypeSource source ) : base( CanInstantiateSpecification.Instance.Inverse() )
			{
				this.source = source;
			}

			static Type Map( Type parameter )
			{
				var name = $"{parameter.Namespace}.{ConventionCandidateNameFactory.Instance.Get( parameter )}";
				var result = name != parameter.FullName ? parameter.Assembly().GetType( name ) : null;
				return result;
			}

			Type Search( Type parameter )
			{
				var adapter = parameter.Adapt();
				var order = Weight( parameter );
				var convention = IsConventionCandidateSpecification.Default.Get( parameter );
				var result =
						source.Get()
						.Where( adapter.IsAssignableFrom )
						.Where( Specification )
						.OrderByDescending( order.GetWeight )
						.FirstOrDefault( convention );
				return result;
			}

			public override Type Get( Type parameter ) => Map( parameter ) ?? Search( parameter );
		}
	}

	class IsConventionCandidateSpecification : SpecificationBase<Type>
	{
		readonly static Func<Type, string> Sanitizer = ConventionCandidateNameFactory.Instance.ToSourceDelegate();

		public static IParameterizedSource<Type, Func<Type, bool>> Default { get; } = new Cache<Type, Func<Type, bool>>( t => new IsConventionCandidateSpecification( t ).IsSatisfiedBy );
		IsConventionCandidateSpecification( Type type ) : this( type, Sanitizer ) {}

		readonly string type;

		public IsConventionCandidateSpecification( Type type, Func<Type, string> sanitizer ) : this( sanitizer( type ) ) {}
		IsConventionCandidateSpecification( string type )
		{
			this.type = type;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter.Name.Equals( type );
	}

	class ConventionCandidateNameFactory : ParameterizedSourceBase<Type, string>
	{
		public static ConventionCandidateNameFactory Instance { get; } = new ConventionCandidateNameFactory();
		ConventionCandidateNameFactory() {}

		public override string Get( Type parameter ) => parameter.Name.TrimStartOf( 'I' );
	}

	public interface ITypeCandidateWeightProvider
	{
		int GetWeight( Type candidate );
	}

	public class TypeCandidateWeightProvider : ParameterizedSourceBase<Type, int>, ITypeCandidateWeightProvider
	{
		readonly Type subject;

		public TypeCandidateWeightProvider( Type subject )
		{
			this.subject = subject;
		}

		public override int Get( Type parameter ) => parameter.IsNested ? subject.GetTypeInfo().DeclaredNestedTypes.Contains( parameter.GetTypeInfo() ) ? 2 : -1 : 0;

		public int GetWeight( Type candidate ) => Get( candidate );
	}

	public sealed class SelfAndNestedTypes : Cache<Type, IEnumerable<Type>>
	{
		public static SelfAndNestedTypes Instance { get; } = new SelfAndNestedTypes();
		SelfAndNestedTypes() : base( type => type.Adapt().WithNested() ) {}
	}

	public class ConventionImplementedInterfaces : FactoryCache<Type, Type>
	{
		public static ConventionImplementedInterfaces Instance { get; } = new ConventionImplementedInterfaces( typeof(ISource), typeof(IParameterizedSource), typeof(IValidatedParameterizedSource) );
		ConventionImplementedInterfaces( params Type[] ignore ) : this( ignore.ToImmutableArray() ) {}

		readonly ImmutableArray<Type> ignore;

		public ConventionImplementedInterfaces( ImmutableArray<Type> ignore )
		{
			this.ignore = ignore;
		}

		protected override Type Create( Type parameter )
		{
			var types = parameter.GetTypeInfo().ImplementedInterfaces.Except( ignore.ToArray() ).ToArray();
			foreach ( var type in types )
			{
				if ( parameter.Name.Contains( type.Name.TrimStartOf( 'I' ) ) )
				{
					return type;
				}
			}
			return null;
		}
	}

	public class CanInstantiateSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Instance { get; } = new CanInstantiateSpecification().Cached();
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
		public static ISpecification<Type> Instance { get; } = new InstantiableTypeSpecification().Cached();
		InstantiableTypeSpecification() : this( new[] { typeof(Delegate), typeof(Array) }.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		readonly ImmutableArray<TypeAdapter> exempt;

		public InstantiableTypeSpecification( ImmutableArray<TypeAdapter> exempt )
		{
			this.exempt = exempt;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter != typeof(object) && !exempt.IsAssignableFrom( parameter );
	}
}
