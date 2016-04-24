using DragonSpark.Extensions;
using DragonSpark.Testing.Framework.Setup.Location;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Testing.Framework.Setup
{
	/*public abstract class AutoDataCommandBase : Command<AutoData> {}*/

	public class SericesCustomization : CustomizationBase
	{
		public static SericesCustomization Instance { get; } = new SericesCustomization();

		protected override void OnCustomize( IFixture fixture ) => fixture.ResidueCollectors.Add( ServiceRelay.Instance );
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