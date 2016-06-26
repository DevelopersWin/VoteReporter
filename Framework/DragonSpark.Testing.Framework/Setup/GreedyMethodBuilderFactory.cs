using System;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using Ploeh.AutoFixture.Kernel;
using PostSharp.Patterns.Contracts;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public abstract class EnginePartFactory<T> : FactoryBase<T, ISpecimenBuilder>, ISpecimenBuilderTransformation where T : ISpecimenBuilder
	{
		readonly Func<T, ISpecimenBuilder> toDelegate;

		protected EnginePartFactory()
		{
			toDelegate = this.ToDelegate();
		}

		public ISpecimenBuilder Transform( ISpecimenBuilder builder ) => builder.AsTo( toDelegate );
	}

	public class OptionalParameterTransformer : EnginePartFactory<Ploeh.AutoFixture.Kernel.ParameterRequestRelay>
	{
		public static OptionalParameterTransformer Instance { get; } = new OptionalParameterTransformer();

		public override ISpecimenBuilder Create( Ploeh.AutoFixture.Kernel.ParameterRequestRelay parameter ) => new ParameterRequestRelay( parameter );
	}

	public class ParameterRequestRelay : ISpecimenBuilder
	{
		readonly Ploeh.AutoFixture.Kernel.ParameterRequestRelay inner;
		readonly static NoSpecimen NoSpecimen = new NoSpecimen();

		public ParameterRequestRelay( [Required]Ploeh.AutoFixture.Kernel.ParameterRequestRelay inner )
		{
			this.inner = inner;
		}

		public object Create( object request, [Required]ISpecimenContext context )
		{
			var parameter = request as ParameterInfo;
			var result = parameter != null ? ( ShouldDefault( parameter ) ? parameter.DefaultValue : inner.Create( request, context ) ) : NoSpecimen;
			return result;
		}

		static bool ShouldDefault( ParameterInfo info ) => 
			info.IsOptional && !GlobalServiceProvider.Instance.Get<AutoData>().Method.GetParameterTypes().Any( info.ParameterType.Adapt().IsAssignableFrom );
	}
}