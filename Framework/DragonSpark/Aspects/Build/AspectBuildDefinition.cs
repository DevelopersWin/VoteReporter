using DragonSpark.Aspects.Definitions;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Aspects;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class AspectBuildDefinition : SpecificationParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>?>, IAspectBuildDefinition
	{
		readonly ImmutableArray<Type> types;

		public AspectBuildDefinition( IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector, params ITypeDefinition[] candidates ) 
			: this( candidates.AsTypes(), selector, candidates ) {}

		AspectBuildDefinition( ImmutableArray<Type> types, IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector, params ITypeDefinition[] candidates ) 
			: this( types, new Implementation( types, selector, candidates ).ToCache() ) {}

		[UsedImplicitly]
		protected AspectBuildDefinition( ImmutableArray<Type> types, IParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>?> instanceSource ) : base( new DelegatedAssignedSpecification<TypeInfo, ImmutableArray<AspectInstance>?>( instanceSource.ToDelegate() ), instanceSource.Get )
		{
			this.types = types;
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => Get( (TypeInfo)targetElement )?.ToArray();

		public IEnumerator<Type> GetEnumerator() => types.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		sealed class Implementation : ParameterizedSourceBase<TypeInfo, ImmutableArray<AspectInstance>?>
		{
			readonly ISpecification<Type> specification;
			readonly IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector;
			readonly ImmutableArray<ITypeDefinition> candidates;

			public Implementation( ImmutableArray<Type> types, IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector, params ITypeDefinition[] candidates ) 
				: this( new CompositeAssignableSpecification( types ), selector, candidates ) {}

			Implementation( ISpecification<Type> specification, IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector, params ITypeDefinition[] candidates )
			{
				this.specification = specification;
				this.selector = selector;
				this.candidates = candidates.ToImmutableArray();
			}

			public override ImmutableArray<AspectInstance>? Get( TypeInfo parameter )
			{
				var builder = ImmutableArray.CreateBuilder<AspectInstance>();
				var valid = parameter.With<Valid>();
				var added = parameter.With<Added>();
				foreach ( var candidate in candidates )
				{
					foreach ( var aspects in selector.GetFixed( candidate ) )
					{
						var isValid = aspects.IsSatisfiedBy( parameter );
						valid.Execute( candidate, aspects, isValid );
						if ( isValid )
						{
							var aspectInstance = aspects.Get( parameter );
							added.Execute( aspects, aspectInstance.AspectTypeName );
							
							builder.Add( aspectInstance );
						}
					}
				}

				var satisfiedBy = specification.IsSatisfiedBy( parameter );
				var result = 
					builder.Any() ? builder.ToImmutable() 
						: satisfiedBy ? Items<AspectInstance>.Immutable : (ImmutableArray<AspectInstance>?)null;
				return result;
			}

			[UsedImplicitly]
			sealed class Valid : LogCommandBase<ITypeDefinition, IAspects, bool>
			{
				public Valid( ILogger logger ) : base( logger.Verbose, "Candidate {TypeDefinition} with Aspects Container {Aspects} is valid: {Valid}" ) {}
			}

			[UsedImplicitly]
			sealed class Added : LogCommandBase<IAspects, string>
			{
				public Added( ILogger logger ) : base( logger.Debug, "{Aspects} applied aspect {Aspect}" ) {}
			}
		}
	}
}