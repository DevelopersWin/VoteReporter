using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup.Commands;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Setup.Registration
{
	public class RegisterFromMetadataCommand : ServicedCommand<MetadataRegistrationCommand, ImmutableArray<Type>> {}

	public class MetadataRegistrationCommand : CommandBase<ImmutableArray<Type>>
	{
		readonly IServiceRegistry registry;

		public MetadataRegistrationCommand( [Required]PersistentServiceRegistry registry )
		{
			this.registry = registry;
		}

		public override void Execute( ImmutableArray<Type> parameter )
		{
			var immutableArray = MetadataRegistrationTypeFactory.Instance.Get( parameter );
			foreach ( var type in immutableArray )
			{
				foreach ( var registration in HostedValueLocator<IRegistration>.Instance.Get( type ) )
				{
					registration.Register( registry );
				}
			}
		}
	}

	public sealed class MetadataRegistrationTypeFactory : TransformerBase<ImmutableArray<Type>>
	{
		public static MetadataRegistrationTypeFactory Instance { get; } = new MetadataRegistrationTypeFactory();
		MetadataRegistrationTypeFactory() {}

		public override ImmutableArray<Type> Get( ImmutableArray<Type> parameter ) => parameter.AsEnumerable().Decorated<RegistrationBaseAttribute>().ToImmutableArray();
	}
}