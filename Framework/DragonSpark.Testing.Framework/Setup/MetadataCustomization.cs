using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using PostSharp.Patterns.Model;

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
		public Application( Assembly[] assemblies, IEnumerable<ICommand> commands ) : base( assemblies, commands.Append( new ApplyExportedCommandsCommand<T>() ) ) {}
	}

	public interface IApplication : DragonSpark.Setup.IApplication, ICommand<AutoData>, IDisposable { }

	public class Application : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		public Application( params ICommand[] commands ) : this( TypeSystem.Assemblies.GetCurrent(), commands ) {}

		public Application( Assembly[] assemblies, IEnumerable<ICommand> commands ) : base( assemblies, commands ) {}

		public void Dispose()
		{
			Commands.OfType<IDisposable>().Reverse().Each( disposable => disposable.Dispose() );
			Commands.Clear();
		}
	}
}