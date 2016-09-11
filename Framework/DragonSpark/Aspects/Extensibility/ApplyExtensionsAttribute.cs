using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Extensibility
{
	[AttributeUsage( AttributeTargets.Class )]
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 6 )]
	public class ApplyExtensionsAttribute : InstanceLevelAspect
	{
		readonly ImmutableArray<IExtension> extensions;

		public ApplyExtensionsAttribute( params Type[] extensionTypes ) : this( extensionTypes.SelectAssigned( Defaults.ExtensionSource ) ) {}

		public ApplyExtensionsAttribute( IEnumerable<IExtension> policies )
		{
			extensions = policies.ToImmutableArray();
		}

		public override void RuntimeInitializeInstance()
		{
			var provider = Instance as IExtensionProvider;
			var provided = provider?.GetExtensions() ?? Items<IExtension>.Default;
			foreach ( var extension in extensions.Concat( provided ).Prioritize() )
			{
				extension.Execute( Instance );
			}
		}
	}

	public interface IExtensionProvider
	{
		IEnumerable<IExtension> GetExtensions();
	}

	public sealed class SpecificationExtension<T> : ExtensionBase
	{
		readonly Invocation invocation;
		readonly Func<Type, IEnumerable<ExtensionPointProfile>> source;

		public SpecificationExtension( ISpecification<T> specification ) : this( specification, ExtensionPointProfiles.DefaultNested.Get ) {}

		public SpecificationExtension( ISpecification<T> specification, Func<Type, IEnumerable<ExtensionPointProfile>> source ) : this( new Invocation( specification ), source ) {}

		SpecificationExtension( Invocation invocation, Func<Type, IEnumerable<ExtensionPointProfile>> source ) : base( Priority.BeforeHigher )
		{
			this.invocation = invocation;
			this.source = source;
		}

		public override void Execute( object parameter )
		{
			var profiles = source( parameter.GetType() );
			foreach ( var pair in profiles )
			{
				pair.Validation.Get( parameter ).Assign( invocation );
			}
		}

		sealed class Invocation : InvocationBase<T, bool>
		{
			readonly ISpecification<T> specification;

			public Invocation( ISpecification<T> specification )
			{
				this.specification = specification;
			}

			public override bool Invoke( T parameter ) => specification.IsSatisfiedBy( parameter );
		}
	}
}