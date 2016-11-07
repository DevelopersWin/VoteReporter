using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	public class Definition<T> : DefinitionBase where T : IAspect
	{
		public Definition( params ITypeDefinition[] definitions ) : this( definitions.AsEnumerable().Concat().ToArray() ) {}
		public Definition( params IMethodStore[] methods ) : this( SpecificationFactory.Default.Get( methods.SelectTypes() ), methods ) {}
		public Definition( Func<Type, bool> specification, params IMethodStore[] methods ) : this( specification, methods.Select( methodStore => new MethodBasedAspectInstanceLocator<T>( methodStore ) ).ToArray() ) {}
		public Definition( Func<Type, bool> specification, params IAspectInstanceLocator[] locators ) : base( specification, locators ) {}
	}
}