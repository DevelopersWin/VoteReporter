using System;
using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup.Commands;
using PostSharp.Patterns.Contracts;
using System.Linq;

namespace DragonSpark.Setup.Registration
{
	public class RegisterFromMetadataCommand : ServicedCommand<MetadataRegistrationCommand, Type[]> {}

	public class MetadataRegistrationCommand : CommandBase<Type[]>
	{
		readonly IServiceRegistry registry;

		public MetadataRegistrationCommand( [Required]PersistentServiceRegistry registry )
		{
			this.registry = registry;
		}

		protected override void OnExecute( Type[] parameter )
		{
			var types = MetadataRegistrationTypeFactory.Instance.Create( parameter );
			types
				.SelectMany( HostedValueLocator<IRegistration>.Instance.Create )
				.Each( registration => registration.Register( registry ) );
		}
	}

	public class MetadataRegistrationTypeFactory : TransformerBase<Type[]>
	{
		public static MetadataRegistrationTypeFactory Instance { get; } = new MetadataRegistrationTypeFactory();

		protected override Type[] CreateItem( Type[] parameter ) => parameter
			.AsTypeInfos()
			.WhereDecorated<RegistrationBaseAttribute>()
			.Select( item => item.Item2 ).AsTypes().Fixed();
	}
}