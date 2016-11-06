using DragonSpark.Commands;
using DragonSpark.Specifications;
using DragonSpark.Windows;
using System;
using System.Configuration;

namespace DevelopersWin.VoteReporter.Parts.Common
{
	public sealed class InitializeUserSettingsCommand : SpecificationCommand<ApplicationSettingsBase>
	{
		public static InitializeUserSettingsCommand Default { get; } = new InitializeUserSettingsCommand();
		InitializeUserSettingsCommand() : base( IsExecutingInManagedHostSpecification.Default.Fixed( AppDomain.CurrentDomain ), DragonSpark.Windows.InitializeUserSettingsCommand.Default.Execute ) {}
	}
}
