using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Extensibility
{
	public sealed class SpecificationExtension<T> : ExtensionBase
	{
		readonly Invocation invocation;
		readonly Func<Type, IEnumerable<ExtensionPointProfile>> source;

		public SpecificationExtension( ISpecification<T> specification ) : this( specification, ExtensionPointProfiles.DefaultNested.Get ) {}

		public SpecificationExtension( ISpecification<T> specification, Func<Type, IEnumerable<ExtensionPointProfile>> source ) : this( new Invocation( specification ), source ) {}

		SpecificationExtension( Invocation invocation, Func<Type, IEnumerable<ExtensionPointProfile>> source )
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