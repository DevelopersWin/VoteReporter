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
	public class RegisterFromMetadataCommand : DelegatedCommand<MetadataRegistrationCommand, Type[]> {}

	public class MetadataRegistrationCommand : Command<Type[]>
	{
		readonly IServiceRegistry registry;

		public MetadataRegistrationCommand( [Required]PersistentServiceRegistry registry )
		{
			this.registry = registry;
		}

		protected override void OnExecute( Type[] parameter ) => 
			parameter
				.AsTypeInfos()
				.WhereDecorated<RegistrationBaseAttribute>()
				.Select( item => item.Item2 )
				.SelectMany( HostedValueLocator<IRegistration>.Instance.Create )
				.Each( registration => registration.Register( registry ) );
	}
}