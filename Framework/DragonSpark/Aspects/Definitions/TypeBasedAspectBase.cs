using DragonSpark.Aspects.Build;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Definitions
{
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class TypeBasedAspectBase : TypeLevelAspect, IAspectProvider
	{
		readonly IAspectBuildDefinition definition;

		protected TypeBasedAspectBase() {}

		protected TypeBasedAspectBase( IAspectBuildDefinition definition )
		{
			this.definition = definition;
		}

		//public override void CompileTimeInitialize( Type type, AspectInfo aspectInfo ) => InitializeDiagnosticsCommand.Default.Execute();

		public override bool CompileTimeValidate( Type type )
		{
			var result = definition.IsSatisfiedBy( type );
			type.With<Valid>().Execute( this, result );
			return result;
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var type = (TypeInfo)targetElement;
			var result = definition.ProvideAspects( targetElement )?.ToArray();
			if ( result == null )
			{
				var types = definition.ToArray();
				var exception = new InvalidOperationException( $"Aspect '{GetType()}' was applied to {targetElement}, but it was not able to apply any aspects to it.  Ensure that {targetElement} implements at least one of these types: {string.Join( ", ", types.Select( t => t.FullName ) )}" );
				type.With<Error>().Execute( exception, this, types );
				throw exception;
			}
			
			type.With<Added>().Execute( this, result.Length, result.Select( instance => instance.AspectTypeName ).Fixed() );

			return result;
		}

		[UsedImplicitly]
		sealed class Valid : LogCommandBase<ITypeLevelAspect, bool>
		{
			public Valid( ILogger logger ) : base( logger.Debug, "{TypeLevelAspect} can provide aspects: {Valid}" ) {}
		}

		[UsedImplicitly]
		sealed class Added : LogCommandBase<ITypeLevelAspect, int, string[]>
		{
			public Added( ILogger logger ) : base( logger.Debug, "{AspectSource} provided {Count} aspects to this element: {Types}" ) {}
		}

		[UsedImplicitly]
		sealed class Error : LogExceptionCommandBase<ITypeLevelAspect, Type[]>
		{
			public Error( ILogger logger ) : base( logger, "No aspects were provided by {AspectSource}, which expects one of the following types to be implemented by the source element: {Types}" ) {}
		}
	}
}