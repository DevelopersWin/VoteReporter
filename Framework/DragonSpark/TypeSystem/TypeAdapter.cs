using DragonSpark.Application;
using DragonSpark.Aspects;
using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem.Generics;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class TypeAssignableSpecification<T> : DelegatedSpecification<Type>
	{
		public static ISpecification<Type> Default { get; } = new TypeAssignableSpecification<T>();
		TypeAssignableSpecification() : base( TypeAssignableSpecification.Delegates.Get( typeof(T) ) ) {}
	}

	public sealed class TypeAssignableSpecification : SpecificationCache<Type>
	{
		public static TypeAssignableSpecification Default { get; } = new TypeAssignableSpecification();
		public static IParameterizedSource<Type, Func<Type, bool>> Delegates { get; } = new Cache<Type, Func<Type, bool>>( Default.To( SpecificationDelegates<Type>.Default ).Get );
		TypeAssignableSpecification() : base( type => new DefaultImplementation( type ).ToCachedSpecification() ) {}

		sealed class DefaultImplementation : TypeSpecificationBase
		{
			public DefaultImplementation( Type context ) : base( context ) {}

			public override bool IsSatisfiedBy( Type parameter ) => 
				Info.IsGenericTypeDefinition && parameter.Adapt().IsGenericOf( Context ) || Info.IsAssignableFrom( parameter.GetTypeInfo() ) || Nullable.GetUnderlyingType( parameter ) == Context;
		}
	}

	public abstract class TypeSpecificationBase : SpecificationWithContextBase<Type>
	{
		protected TypeSpecificationBase( Type context ) : this( context, context.GetTypeInfo() ) {}

		[UsedImplicitly]
		protected TypeSpecificationBase( Type context, TypeInfo info ) : base( context )
		{
			Info = info;
		}

		protected TypeInfo Info { get; }
	}

	public static partial class Extensions
	{
		public static IGenericMethodContext<Invoke> GetFactory( this Type @this, string methodName ) => GenericFactories.Default.Get( @this )[methodName];
		sealed class GenericFactories : DecoratedCache<Type, GenericStaticMethodFactories>
		{
			public static GenericFactories Default { get; } = new GenericFactories();
			GenericFactories() {}
		}

		public static IGenericMethodContext<Execute> GetCommand( this Type @this, string methodName ) => GenericCommands.Default.Get( @this )[methodName];
		sealed class GenericCommands : DecoratedCache<Type, GenericStaticMethodCommands>
		{
			public static GenericCommands Default { get; } = new GenericCommands();
			GenericCommands() {}
		}

		public static ImmutableArray<Type> WithNested( this Type @this ) => NestedTypes.Default.Get( @this );

		public static bool IsAssignableFrom( this Type @this, Type other ) => TypeAssignableSpecification.Default.Get( @this ).IsSatisfiedBy( other );
		
		public static bool IsInstanceOfType( this Type @this, object instance ) => @this.IsAssignableFrom( instance.GetType() );

		public static Type GetInnerType( this Type @this ) => InnerTypes.Default.Get( @this );
		public static Type GetEnumerableType( this Type @this ) => EnumerableTypes.Default.Get( @this );
		public static ImmutableArray<Type> GetHierarchy( this Type @this ) => TypeHierarchies.Default.Get( @this );
		public static ImmutableArray<Type> GetAllInterfaces( this Type @this ) => Interfaces.Default.Get( @this );
	}

	public sealed class NestedTypes : CacheWithImplementedFactoryBase<TypeInfo, ImmutableArray<Type>>
	{
		readonly static Func<TypeInfo, bool> Specification = ApplicationTypeSpecification.Default.IsSatisfiedBy;

		public static NestedTypes Default { get; } = new NestedTypes();
		NestedTypes() {}

		protected override ImmutableArray<Type> Create( TypeInfo parameter ) =>
			parameter.Append( parameter.DeclaredNestedTypes ).Where( Specification ).AsTypes().ToImmutableArray();
	}

	public sealed class ConstructorLocator : ParameterizedSourceCache<TypeInfo, ImmutableArray<Type>, ConstructorInfo>
	{
		public static ConstructorLocator Default { get; } = new ConstructorLocator();
		ConstructorLocator() : base( info => new ExtendedDictionaryCache<ImmutableArray<Type>, ConstructorInfo>( new DefaultImplementation( info ).Get ) ) {}

		sealed class DefaultImplementation : ParameterizedSourceBase<ImmutableArray<Type>, ConstructorInfo>
		{
			ImmutableArray<ConstructorInfo> candidates;

			public DefaultImplementation( TypeInfo typeInfo ) : this( InstanceConstructors.Default.Get( typeInfo ) ) {}

			DefaultImplementation( ImmutableArray<ConstructorInfo> candidates )
			{
				this.candidates = candidates;
			}

			public override ConstructorInfo Get( ImmutableArray<Type> parameter ) =>
				candidates
					.Introduce( parameter, tuple => CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ) )
					.SingleOrDefault();
		}
	}

	public sealed class InnerTypes : Cache<Type, Type>
	{
		public static InnerTypes Default { get; } = new InnerTypes();
		InnerTypes() : base( TypeLocator.Default.Get ) {}
	}

	public sealed class EnumerableTypes : Cache<Type, Type>
	{
		public static EnumerableTypes Default { get; } = new EnumerableTypes();
		EnumerableTypes() : base( new TypeLocator( i => i.Adapt().IsGenericOf( typeof(IEnumerable<>) ) ).Get ) {}
	}

	public sealed class TypeLocator : AlterationBase<Type>
	{
		readonly Func<Type, ImmutableArray<Type>> typeSource;
		readonly Func<TypeInfo, bool> where;
		readonly Func<Type[], Type> selector;

		public static TypeLocator Default { get; } = new TypeLocator();
		TypeLocator() : this( Where<TypeInfo>.Always ) {}

		public TypeLocator( Func<TypeInfo, bool> where ) : this( TypeHierarchies.Default.Get, where, types => types.Only() ) {}

		public TypeLocator( Func<Type, ImmutableArray<Type>> typeSource, Func<TypeInfo, bool> where, Func<Type[], Type> selector )
		{
			this.typeSource = typeSource;
			this.where = where;
			this.selector = selector;
		}

		public override Type Get( Type parameter )
		{
			foreach ( var type in typeSource( parameter ) )
			{
				var info = type.GetTypeInfo();
				var result = info.IsGenericType && info.GenericTypeArguments.Any() && where( info ) ? selector( info.GenericTypeArguments ) :
					type.IsArray ? type.GetElementType() : null;
				if ( result != null )
				{
					return result;
				}
			}
			return null;
		}
	}

	public sealed class Interfaces : ParameterizedItemCache<Type, Type>
	{
		public static Interfaces Default { get; } = new Interfaces();
		Interfaces() : base( DefaultImplementation.Implementation ) {}

		public sealed class DefaultImplementation : ParameterizedItemSourceBase<Type, Type>
		{
			readonly Func<Type, IEnumerable<Type>> selector;

			public static DefaultImplementation Implementation { get; } = new DefaultImplementation();
			DefaultImplementation()
			{
				selector = Yield;
			}

			public override IEnumerable<Type> Yield( Type parameter ) => 
				parameter
					.Append( parameter.GetTypeInfo().ImplementedInterfaces.SelectMany( selector ) )
					.Where( x => x.GetTypeInfo().IsInterface )
					.Distinct();
		}
	}

	public sealed class TypeHierarchies : ParameterizedItemCache<TypeInfo, Type>
	{
		public static TypeHierarchies Default { get; } = new TypeHierarchies();
		TypeHierarchies() : base( DefaultImplementation.Implementation ) {}

		public sealed class DefaultImplementation : ParameterizedItemSourceBase<TypeInfo, Type>
		{
			public static DefaultImplementation Implementation { get; } = new DefaultImplementation();
			DefaultImplementation() : this( typeof(object) ) {}

			readonly Type rootType;

			[UsedImplicitly]
			public DefaultImplementation( Type rootType )
			{
				this.rootType = rootType;
			}

			public override IEnumerable<Type> Yield( TypeInfo parameter )
			{
				yield return parameter.AsType();
				var current = parameter.BaseType;
				while ( current != null )
				{
					if ( current != rootType )
					{
						yield return current;
					}
					current = current.GetTypeInfo().BaseType;
				}
			}
		}
	}

	public sealed class TypeAdapter : ITypeAware
	{
		// readonly static Func<Type, IEnumerable<Type>> Expand = ExpandInterfaces;
		// readonly Func<Type, bool> isAssignableFrom;
		
		readonly Func<Type, ImmutableArray<MethodMapping>> methodMapper;
		public TypeAdapter( Type referencedType ) : this( referencedType, referencedType.GetTypeInfo() ) {}

		public TypeAdapter( Type referencedType, TypeInfo info )
		{
			ReferencedType = referencedType;
			Info = info;
			methodMapper = new DecoratedSourceCache<Type, ImmutableArray<MethodMapping>>( new MethodMapper( this ).Get ).Get;
		}

		public Type ReferencedType { get; }

		public TypeInfo Info { get; }

		[Freeze]
		public Type[] GetImplementations( Type genericDefinition )
		{
			var result = ReferencedType.Append( Interfaces.Default.GetFixed( ReferencedType ) )
							 .Introduce( genericDefinition, tuple =>
															{
																var first = tuple.Item1.GetTypeInfo();
																var second = tuple.Item2.GetTypeInfo();
																var match = first.IsGenericType && second.IsGenericType && tuple.Item1.GetGenericTypeDefinition() == tuple.Item2.GetGenericTypeDefinition();
																return match;
															} )
							 .Fixed();
			return result;
		}

		public ImmutableArray<MethodMapping> GetMappedMethods( Type interfaceType ) => methodMapper( interfaceType );
		

		[Freeze]
		public bool IsGenericOf( Type genericDefinition ) => GetImplementations( genericDefinition ).Any();

		/*[Freeze]
		public Type[] GetAllInterfaces() => Expand( ReferencedType ).ToArray();

		static IEnumerable<Type> ExpandInterfaces( Type target ) => target.Append( target.GetTypeInfo().ImplementedInterfaces.SelectMany( Expand ) ).Where( x => x.GetTypeInfo().IsInterface ).Distinct();*/
	}
}