using DragonSpark.Runtime.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects
{
	[LinesOfCodeAvoided( 4 )]
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class SpecificationBasedAspect : MethodInterceptionAspect
	{
		readonly ISpecification specification;

		protected SpecificationBasedAspect( ISpecification specification )
		{
			this.specification = specification;
		}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( specification.IsSatisfiedBy( this ) )
			{
				base.OnInvoke( args );
			}
		}
	}


	public sealed class Runtime : SpecificationBasedAspect
	{
		public Runtime() : base( RuntimeSpecification.Instance ) {}
	}

	public class RuntimeSpecification : SpecificationBase<object>
	{
		public static RuntimeSpecification Instance { get; } = new RuntimeSpecification();

		public override bool IsSatisfiedBy( object parameter ) => !PostSharpEnvironment.IsPostSharpRunning;
	}
}
