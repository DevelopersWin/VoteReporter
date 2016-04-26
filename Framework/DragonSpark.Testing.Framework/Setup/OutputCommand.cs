using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework.Setup.Location;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ServicesCustomization : CustomizationBase
	{
		public static ServicesCustomization Instance { get; } = new ServicesCustomization();

		protected override void OnCustomize( IFixture fixture )
		{
			fixture.Customizations.Insert( 0, FrameworkSpecimenBuilder.Instance );
			fixture.ResidueCollectors.Add( ServiceRelay.Instance );
		}

		public class FrameworkSpecimenBuilder : ISpecimenBuilder
		{
			public static FrameworkSpecimenBuilder Instance { get; } = new FrameworkSpecimenBuilder();

			readonly Type[] types;

			public FrameworkSpecimenBuilder() : this( new[] { typeof(Type[]), typeof(Assembly[]) } ) {}

			public FrameworkSpecimenBuilder( Type[] types )
			{
				this.types = types;
			}

			public object Create( object request, ISpecimenContext context ) => TypeSupport.From( request ).With( type => types.Contains( type ) ? Services.Get( type ) : null ) ?? new NoSpecimen();
		}
	}

	public class ServiceRelay : ISpecimenBuilder
	{
		public static ServiceRelay Instance { get; } = new ServiceRelay();

		readonly Func<Type, object> provider;

		public ServiceRelay() : this( Activation.Services.Get ) {}

		public ServiceRelay( [Required]Func<Type, object> provider )
		{
			this.provider = provider;
		}

		public object Create( object request, ISpecimenContext context )
		{
			var type = TypeSupport.From( request );
			var result = type.With( provider ) ?? new NoSpecimen();
			return result;
		}
	}

	/*public class OutputCommand : AutoDataCommand
	{
		[Service]
		public ILoggerHistory History { [return: Required]get; set; }

		protected override void OnExecute( AutoData parameter )
		{
			var declaringType = parameter.Method.DeclaringType;
			if ( declaringType.GetConstructors().Where( info => info.IsPublic ).Any( info => info.GetParameters().Any( parameterInfo => parameterInfo.ParameterType == typeof(ITestOutputHelper) ) ) )
			{
				var lines = LogEventMessageFactory.Instance.Create( History.Events );
				new OutputValue( declaringType ).Assign( lines );	
			}
		}
	}*/
}