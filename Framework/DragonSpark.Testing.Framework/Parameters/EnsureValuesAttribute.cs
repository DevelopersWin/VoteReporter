using DragonSpark.ComponentModel;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Parameters
{
	[AttributeUsage( AttributeTargets.Parameter )]
	public class ServiceAttribute : CustomizeAttribute
	{
		public override ICustomization GetCustomization( ParameterInfo parameter ) => new ServiceRegistration( parameter.ParameterType );
	}

	[AttributeUsage( AttributeTargets.Parameter )]
	public class EnsureValuesAttribute : CustomizeAttribute
	{
		class Customization : ICustomization
		{
			readonly Type requestType;

			public Customization( Type requestType )
			{
				this.requestType = requestType;
			}

			public void Customize( IFixture fixture ) => fixture.Behaviors.Add( new Builder( requestType ) );

			sealed class Builder : ISpecimenBuilderTransformation, ISpecimenCommand
			{
				readonly Type type;
				readonly static Func<PropertyInfo, bool> IsSatisfiedBy = DefaultValuePropertySpecification.Instance.IsSatisfiedBy;

				public Builder( Type type )
				{
					this.type = type;
				}

				public ISpecimenBuilder Transform( ISpecimenBuilder builder ) => new Postprocessor( builder, this );

				public void Execute( object specimen, ISpecimenContext context )
				{
					if ( type.IsInstanceOfType( specimen ) )
					{
						foreach ( var source in specimen.GetType().GetRuntimeProperties().Where( IsSatisfiedBy ) )
						{
							source.GetValue( specimen );
						}
					}
				}
			}
		}

		public override ICustomization GetCustomization( ParameterInfo parameter ) => new Customization( parameter.ParameterType );
	}
}