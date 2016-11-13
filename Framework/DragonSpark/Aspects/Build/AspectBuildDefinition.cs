using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	/*public static class Defaults
	{
		public static ISpecification<TypeInfo> Instantiable { get; } = Activation.Defaults.Instantiable.Coerce( AsTypeCoercer.Default );

		public static ISpecification<TypeInfo> Introduce { get; } = Instantiable.And( FirstInstantiable.Default );
	}*/

	public class AspectBuildDefinition : SpecificationParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>?>, IAspectBuildDefinition
	{
		readonly ImmutableArray<Type> types;

		public AspectBuildDefinition( IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector, params ITypeDefinition[] candidates ) 
			: this( candidates.AsTypes(), selector, candidates ) {}

		/*public AspectBuildDefinition( ISpecification<TypeInfo> specification, IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectDefinition>> selector, params ITypeDefinition[] candidates ) 
			: this( candidates.AsTypes(), specification, selector, candidates ) {}*/

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
			readonly ImmutableArray<Type> types;
			readonly ISpecification<TypeInfo> specification;
			readonly IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector;
			readonly ImmutableArray<ITypeDefinition> candidates;

			public Implementation( ImmutableArray<Type> types, IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector, params ITypeDefinition[] candidates ) 
				: this( types, new AdapterAssignableSpecification( types.ToArray() ), selector, candidates ) {}

			Implementation( ImmutableArray<Type> types, ISpecification<TypeInfo> specification, IParameterizedSource<ITypeDefinition, ImmutableArray<IAspects>> selector, params ITypeDefinition[] candidates )
			{
				this.types = types;
				this.specification = specification;
				this.selector = selector;
				this.candidates = candidates.ToImmutableArray();
			}

			public override ImmutableArray<AspectInstance>? Get( TypeInfo parameter )
			{
				var builder = ImmutableArray.CreateBuilder<AspectInstance>();
				foreach ( var candidate in candidates )
				{
					var selectors = selector.GetFixed( candidate );

					foreach ( var item in selectors )
					{
						var isSatisfiedBy = item.IsSatisfiedBy( parameter );
						MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"Satisfies: {parameter} => {candidate} - {item} - Valid: {isSatisfiedBy}", null, null, null ));
						if ( isSatisfiedBy )
						{
							var aspectInstance = item.Get( parameter );
							MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"ADD: {parameter} => {candidate} - {item} - Applied {aspectInstance.AspectTypeName} on {aspectInstance.TargetElement}", null, null, null ));
							
							builder.Add( aspectInstance );
						}
					}
				}

				var satisfiedBy = specification.IsSatisfiedBy( parameter );
				var result = 
					builder.Any() ? builder.ToImmutable() 
						: satisfiedBy ? Items<AspectInstance>.Immutable : (ImmutableArray<AspectInstance>?)null;
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"FAILURE: {parameter} {candidates.First()} => {satisfiedBy} = {string.Join( ", ", types.Select( t => t.FullName ) )}", null, null, null ));
				return result;
			}
		}
	}
}