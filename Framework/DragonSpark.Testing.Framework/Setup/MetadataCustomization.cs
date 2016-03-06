using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCustomization : AutoDataCustomization
	{
		public static MetadataCustomization Instance { get; } = new MetadataCustomization();

		readonly Func<MethodBase, ICustomization[]> factory;

		public MetadataCustomization() : this( MetadataCustomizationFactory.Instance.Create ) {}

		public MetadataCustomization( Func<MethodBase, ICustomization[]> factory )
		{
			this.factory = factory;
		}

		protected override void OnInitializing( AutoData context ) => factory( context.Method ).Each( customization => customization.Customize( context.Fixture ) );
	}

	public class Application<T> : Application where T : ICommand
	{
		public Application( Assembly[] assemblies, params ICommand<AutoData>[] commands ) : base( assemblies, commands.Concat( new [] { new ApplyExportedCommandsCommand<T>() } ).ToArray() ) {}
	}

	public class Application : DragonSpark.Setup.Application<AutoData>
	{
		public Application( params ICommand<AutoData>[] commands ) : this( Default<Assembly>.Items, commands ) {}

		public Application( [Required] Assembly[] assemblies, params ICommand<AutoData>[] commands ) : base( commands )
		{
			Assemblies = assemblies;
		}

		protected override void OnExecute( AutoData parameter )
		{
			using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( parameter.Method ) ) )
			{
				using ( new AssignAutoDataCommand().ExecuteWith( parameter ) )
				{
					base.OnExecute( parameter );
				}
			}
		}

		protected override void OnExecuteCore( AutoData parameter )
		{
			base.OnExecuteCore( parameter );
			parameter.Apply();
		}
	}

	/*public abstract class ApplicationFactory<TSetup> : FactoryBase<Application> where TSetup : ICommand
	{
		readonly Assembly[] assemblies;
		readonly ICommand<object>[] commands;

		protected ApplicationFactory( Assembly[] assemblies, params ICommand<object>[] commands )
		{
			this.assemblies = assemblies;
			this.commands = commands.Concat( new [] { new ApplyExportedCommandsCommand<TSetup>() } ).ToArray();
		}

		protected override Application CreateItem() => new Application( assemblies, commands );
	}*/

	/*public class ApplicationCustomization : AutoDataCustomization
	{
		readonly Func<ICommand<AutoData>> setupFactory;
	
		protected ApplicationCustomization( Func<ICommand<AutoData>> setupFactory )
		{
			this.setupFactory = setupFactory;
		}

		protected override void OnInitializing( AutoData context )
		{
			var setup = setupFactory();
			setup.Run( context );
		}
	}*/
}