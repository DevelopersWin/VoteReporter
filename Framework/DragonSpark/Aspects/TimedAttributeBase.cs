using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class TimedAttributeBase : MethodInterceptionAspect
	{
		readonly string template;

		protected TimedAttributeBase() : this( "Executed Method '{@Method}'" ) {}

		protected TimedAttributeBase( string template )
		{
			this.template = template;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			// using ( Logger.Default.Get( args.Method ).TimeOperation( template, args.Method ) )
			{
				base.OnInvoke( args );
			}
		}
	}
}