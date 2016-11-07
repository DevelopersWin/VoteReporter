using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	public class AspectBuildDefinition<T> : AspectBuildDefinitionBase where T : IAspect
	{
		public AspectBuildDefinition( params ITypeDefinition[] definitions ) : this( definitions.AsEnumerable().Concat().ToArray() ) {}
		public AspectBuildDefinition( params IMethodStore[] methods ) : this( SpecificationFactory.Default.Get( methods.SelectTypes() ), methods ) {}
		public AspectBuildDefinition( Func<Type, bool> specification, params IMethodStore[] methods ) : this( specification, methods.Select( methodStore => new MethodBasedAspectInstanceLocator<T>( methodStore ) ).ToArray() ) {}
		public AspectBuildDefinition( Func<Type, bool> specification, params IAspectInstanceLocator[] locators ) : base( specification, locators ) {}
	}
}