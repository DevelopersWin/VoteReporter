using DragonSpark.Activation;
using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Delegates;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Composition
{
	public sealed class ExportsProfileFactory : SourceBase<ExportsProfile>
	{
		readonly static Func<Type, ConventionMapping> Selector = ConventionMappingFactory.Default.Get;

		public static ISource<ExportsProfile> Default { get; } = new Scope<ExportsProfile>( new ExportsProfileFactory().Global() );
		ExportsProfileFactory() : this( ApplicationTypes.Default.ToDelegate(), AppliedExportLocator.Default.ToSourceDelegate() ) {}

		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<Type, AppliedExport> exportSource;

		ExportsProfileFactory( Func<ImmutableArray<Type>> typesSource, Func<Type, AppliedExport> exportSource )
		{
			this.typesSource = typesSource;
			this.exportSource = exportSource;
		}

		public override ExportsProfile Get()
		{
			var types = typesSource();
			
			var applied = new AppliedExports( types.Select( exportSource ).WhereAssigned() );
			var appliedTypes = applied.All().ToArray();

			var mappings = new ConventionMappings( types.Except( appliedTypes )
														.Select( Selector )
														.WhereAssigned()
														.Distinct( mapping => mapping.InterfaceType )
						   );
				

			/*Debug.WriteLine( "Defined:" );
			foreach ( var pair in defined )
			{
				Debug.WriteLine( $"[{pair.Subject}, {pair.ExportAs}]" );
			}

			Debug.WriteLine( "Conventions:" );
			foreach ( var pair in mappings )
			{
				Debug.WriteLine( $"{pair.InterfaceType.GetTypeInfo().IsPublic}, {pair.ImplementationType.GetTypeInfo().IsPublic}: [{pair.InterfaceType}, {pair.ImplementationType}]" );
			}*/

			var all = appliedTypes.Concat( mappings.All() ).SelectMany( ExportTypeExpander.Default.Get ).Distinct().ToImmutableHashSet();
			var selector = new ConstructorSelector( new IsValidConstructorSpecification( new IsValidTypeSpecification( all ).IsSatisfiedBy ).IsSatisfiedBy );

			var implementations = mappings.Values.Fixed();
			var constructions = applied
				.GetClasses()
				.Union( implementations )
				.Select( selector.Get )
				.WhereAssigned()
				.ToDictionary( info => info.DeclaringType );

			var constructed = new ConstructedExports( constructions );

			var conventions = new ConventionExports( mappings.Keys, implementations.Intersect( constructions.Keys ) );

			var singletons = new SingletonExports( implementations.Except( constructions.Keys ).Union( applied.GetProperties() ).Select( SingletonExportFactory.Default.Get ).WhereAssigned().ToDictionary( export => export.Location.DeclaringType ) );

			var result = new ExportsProfile( constructed, conventions, singletons );
			return result;
		}
	}

	public sealed class SingletonExports : ExportSourceBase<SingletonExport>, IEnumerable<SingletonExport>
	{
		readonly IDictionary<Type, SingletonExport> dictionary;
		public SingletonExports( IDictionary<Type, SingletonExport> dictionary ) : base( dictionary.Keys )
		{
			this.dictionary = dictionary;
		}
		public override SingletonExport Get( Type parameter ) => dictionary[ parameter ];
		public IEnumerator<SingletonExport> GetEnumerator() => dictionary.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public sealed class ConventionExports : ExportSourceBase<bool>
	{
		readonly ImmutableArray<Type> interfaces;

		public ConventionExports( IEnumerable<Type> interfaces, IEnumerable<Type> types ) : base( types )
		{
			this.interfaces = interfaces.ToImmutableArray();
		}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var isSatisfiedBy = base.IsSatisfiedBy( parameter );
			return isSatisfiedBy;
		}

		public override bool Get( Type parameter )
		{
			var contains = interfaces.Contains( parameter );
			return contains;
		}
	}

	public abstract class ExportSourceBase<T> : ValidatedParameterizedSourceBase<Type, T>
	{
		readonly ImmutableArray<Type> types;

		protected ExportSourceBase( IEnumerable<Type> types )
		{
			this.types = types.ToImmutableArray();
		}

		public override bool IsSatisfiedBy( Type parameter ) => types.Contains( parameter );
	}

	// public sealed class ConstructorSelector : 

	public sealed class ConstructedExports : ExportSourceBase<ConstructorInfo>
	{
		readonly IDictionary<Type, ConstructorInfo> constructors;
		
		public ConstructedExports( IDictionary<Type, ConstructorInfo> constructors ) : base( constructors.Keys )
		{
			this.constructors = constructors;
		}

		public ConstructorInfo Get( IEnumerable<ConstructorInfo> parameter ) => Get( parameter.Select( info => info.DeclaringType ).Distinct().Single() );
		public override ConstructorInfo Get( Type parameter ) => constructors[ parameter ];
	}

	sealed class ExportTypeExpander : ParameterizedSourceBase<Type, IEnumerable<Type>>
	{
		public static ExportTypeExpander Default { get; } = new ExportTypeExpander();
		ExportTypeExpander() {}

		public override IEnumerable<Type> Get( Type parameter )
		{
			yield return parameter;
			var provider = Activator.Default.Provider();
			var sourceType = SourceTypeLocator.Default.Get( parameter );
			if ( sourceType != null )
			{
				yield return ResultTypes.Default.Get( sourceType );
				yield return ParameterizedSourceDelegates.Sources.Get( provider ).Get( sourceType )?.GetType() ?? SourceDelegates.Sources.Get( provider ).Get( sourceType )?.GetType();
			}
		}
	}

	public sealed class ConventionMappings : DescriptorCollectionBase<ConventionMapping>
	{
		public ConventionMappings( IEnumerable<ConventionMapping> items ) : base( items, mapping => mapping.ImplementationType ) {}

		protected override Type GetKeyForItem( ConventionMapping item ) => item.InterfaceType;
	}

	public abstract class DescriptorCollectionBase<T> : KeyedCollection<Type, T>
	{
		readonly Func<T, Type> valueSource;

		protected DescriptorCollectionBase( IEnumerable<T> items, Func<T, Type> valueSource )
		{
			this.valueSource = valueSource;
			this.AddRange( items );
		}

		public IEnumerable<Type> Keys => Dictionary?.Keys ?? Items<Type>.Default;

		public IEnumerable<Type> Values => Dictionary?.Values.Select( valueSource ) ?? Items<Type>.Default;

		public IEnumerable<Type> All() => Keys.Concat( Values ).Distinct();
	}

	public sealed class AppliedExports : DescriptorCollectionBase<AppliedExport>
	{
		public AppliedExports( IEnumerable<AppliedExport> items ) : base( items, descriptor => descriptor.ExportAs ) {}

		protected override Type GetKeyForItem( AppliedExport item ) => item.Subject;

		public IEnumerable<Type> GetClasses() => Get<TypeInfo>();

		public IEnumerable<Type> GetProperties() => Get<PropertyInfo>();

		IEnumerable<Type> Get<T>() where T : MemberInfo => from item in Items where item.Location is T select item.Subject;
	}

	public struct ExportsProfile 
	{
		public ExportsProfile( ConstructedExports constructed, ConventionExports convention, SingletonExports singletons )
		{
			Constructed = constructed;
			Convention = convention;
			Singletons = singletons;
		}

		public ConstructedExports Constructed { get; }
		public ConventionExports Convention { get; }
		public SingletonExports Singletons { get; }
	}
}