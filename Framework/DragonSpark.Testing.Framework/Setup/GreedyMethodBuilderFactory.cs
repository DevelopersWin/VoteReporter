using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using Ploeh.AutoFixture.Kernel;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public abstract class EnginePartFactory<T> : ParameterizedSourceBase<T, ISpecimenBuilder>, ISpecimenBuilderTransformation where T : ISpecimenBuilder
	{
		readonly Func<T, ISpecimenBuilder> toDelegate;

		protected EnginePartFactory()
		{
			toDelegate = this.ToSourceDelegate();
		}

		public ISpecimenBuilder Transform( ISpecimenBuilder builder ) => builder.AsTo( toDelegate );
	}

	public class OptionalParameterTransformer : EnginePartFactory<Ploeh.AutoFixture.Kernel.ParameterRequestRelay>
	{
		public static OptionalParameterTransformer Default { get; } = new OptionalParameterTransformer();

		public override ISpecimenBuilder Get( Ploeh.AutoFixture.Kernel.ParameterRequestRelay parameter ) => new ParameterRequestRelay( parameter );
	}

	public class ParameterRequestRelay : ISpecimenBuilder
	{
		readonly Ploeh.AutoFixture.Kernel.ParameterRequestRelay inner;
		readonly static NoSpecimen NoSpecimen = new NoSpecimen();

		public ParameterRequestRelay( Ploeh.AutoFixture.Kernel.ParameterRequestRelay inner )
		{
			this.inner = inner;
		}

		public object Create( object request, ISpecimenContext context )
		{
			var parameter = request as ParameterInfo;
			var result = parameter != null ? ( ShouldDefault( parameter ) ? parameter.DefaultValue : inner.Create( request, context ) ) : NoSpecimen;
			return result;
		}

		static bool ShouldDefault( ParameterInfo info ) => 
			info.IsOptional && !MethodContext.Default.Get().GetParameterTypes().Any( info.ParameterType.Adapt().IsAssignableFrom );
	}
}