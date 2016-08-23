using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework.Setup.Location;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ServicesCustomization : CustomizationBase
	{
		public static ServicesCustomization Default { get; } = new ServicesCustomization();

		protected override void OnCustomize( IFixture fixture )
		{
			fixture.Customizations.Insert( 0, FrameworkSpecimenBuilder.DefaultNested );
			fixture.ResidueCollectors.Add( ServiceRelay.Default );
		}

		class FrameworkSpecimenBuilder : ISpecimenBuilder
		{
			public static FrameworkSpecimenBuilder DefaultNested { get; } = new FrameworkSpecimenBuilder();
			FrameworkSpecimenBuilder() : this( new[] { typeof(Type[]), typeof(Assembly[]), typeof(ImmutableArray<Type>), typeof(ImmutableArray<Assembly>) } ) {}

			readonly Type[] types;

			FrameworkSpecimenBuilder( Type[] types )
			{
				this.types = types;
			}

			public object Create( object request, ISpecimenContext context )
			{
				var type = TypeSupport.From( request );
				var result = type != null && types.Contains( type ) ? GlobalServiceProvider.GetService<object>( type ) : new NoSpecimen();
				return result;
			}
		}
	}

	public class ServiceRelay : ISpecimenBuilder
	{
		public static ServiceRelay Default { get; } = new ServiceRelay();
		ServiceRelay() : this( GlobalServiceProvider.GetService<object> ) {}

		readonly Func<Type, object> provider;

		public ServiceRelay( Func<Type, object> provider )
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

		public override void Execute( AutoData parameter )
		{
			var declaringType = parameter.Method.DeclaringType;
			if ( declaringType.GetConstructors().Where( info => info.IsPublic ).Any( info => info.GetParameters().Any( parameterInfo => parameterInfo.ParameterType == typeof(ITestOutputHelper) ) ) )
			{
				var lines = LogEventMessageFactory.Default.Create( History.Events );
				new OutputValue( declaringType ).Assign( lines );	
			}
		}
	}*/
}