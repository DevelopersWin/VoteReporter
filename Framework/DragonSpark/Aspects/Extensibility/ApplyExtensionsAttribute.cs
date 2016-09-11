using DragonSpark.Extensions;
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
	// [MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[LinesOfCodeAvoided( 6 )]
	public class ApplyExtensionsAttribute : InstanceLevelAspect
	{
		// readonly static Action<Type> Command = ApplyPoliciesCommand.Default.Execute;

		readonly ImmutableArray<IExtension> extensions;

		public ApplyExtensionsAttribute( params Type[] extensionTypes ) : this( extensionTypes.SelectAssigned( Defaults.ExtensionSource ) ) {}

		public ApplyExtensionsAttribute( IEnumerable<IExtension> policies )
		{
			this.extensions = policies.ToImmutableArray();
		}

		public override void RuntimeInitializeInstance()
		{
			foreach ( var extension in extensions )
			{
				extension.Execute( Instance );
			}

			var provider = Instance as IExtensionProvider;
			if ( provider != null )
			{
				foreach ( var extension in provider.GetExtensions() )
				{
					extension.Execute( Instance );
				}
			}
		}
	}

	public interface IExtensionProvider
	{
		IEnumerable<IExtension> GetExtensions();
	}

	class SpecificationExtension : IExtension
	{
		public void Execute( object parameter ) {}
	}
}