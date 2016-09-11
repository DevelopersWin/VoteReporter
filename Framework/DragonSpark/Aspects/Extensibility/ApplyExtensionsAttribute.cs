using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Extensibility
{
	[LinesOfCodeAvoided( 6 )]
	public class ApplyExtensionsAttribute : EnableExtensionsAttribute
	{
		readonly ImmutableArray<IExtension> extensions;

		public ApplyExtensionsAttribute( params Type[] extensionTypes ) : this( extensionTypes.SelectAssigned( Defaults.ExtensionSource ).Fixed() ) {}

		protected ApplyExtensionsAttribute( params IExtension[] extensions )
		{
			this.extensions = extensions.ToImmutableArray();
		}

		public override void RuntimeInitializeInstance()
		{
			foreach ( var extension in extensions )
			{
				extension.Execute( Instance );
			}
		}
	}

	public interface IExtensionAware {}

	[IntroduceInterface( typeof(IExtensionAware) )]
	[AttributeUsage( AttributeTargets.Class )]
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public class EnableExtensionsAttribute : InstanceLevelAspect, IExtensionAware {}
}