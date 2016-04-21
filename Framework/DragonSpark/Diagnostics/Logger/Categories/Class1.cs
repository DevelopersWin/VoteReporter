using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using Serilog;
using Serilog.Events;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Diagnostics.Logger.Categories
{
	public abstract class CategoryFactory : FactoryBase<ILogger, LogTemplate> {}

	public class Debug : CategoryFactory
	{
		public static Debug Instance { get; } = new Debug();

		protected override LogTemplate CreateItem( ILogger parameter ) => parameter.Debug;
	}

	public class CategoryFactorySelector : FactoryBase<LogEventLevel, CategoryFactory>
	{
		public static CategoryFactorySelector Instance { get; } = new CategoryFactorySelector();

		readonly IActivator activator;

		public CategoryFactorySelector() : this( Activator.Instance ) {}

		public CategoryFactorySelector( IActivator activator )
		{
			this.activator = activator;
		}

		protected override CategoryFactory CreateItem( LogEventLevel parameter )
		{
			var type = GetType();
			var name = $"{type.Namespace}.{parameter}";
			var result = type.Assembly().DefinedTypes.Only( info => info.FullName == name ).With( info => info.AsType().With( activator.Activate<CategoryFactory> ) );
			return result;
		}
	}

	public class Configured : CategoryFactory
	{
		public static Configured Instance { get; } = new Configured();

		readonly Func<LogEventLevel> levelSource;
		readonly Func<LogEventLevel, CategoryFactory> source;

		public Configured() : this( FrameworkConfiguration.Factory( configuration => configuration.Diagnostics.MinimumLevel ), CategoryFactorySelector.Instance.Create ) {}

		public Configured( Func<LogEventLevel> levelSource, Func<LogEventLevel, CategoryFactory> source )
		{
			this.levelSource = levelSource;
			this.source = source;
		}

		protected override LogTemplate CreateItem( ILogger parameter )
		{
			var level = levelSource();
			var result = source( level ).Create( parameter );
			return result;
		}
	}

	public class Information : CategoryFactory
	{
		public static Information Instance { get; } = new Information();

		protected override LogTemplate CreateItem( ILogger parameter ) => parameter.Information;
	}

	public class Warning : CategoryFactory
	{
		public static Warning Instance { get; } = new Warning();

		protected override LogTemplate CreateItem( ILogger parameter ) => parameter.Warning;
	}

	public class Error : CategoryFactory
	{
		public static Error Instance { get; } = new Error();

		protected override LogTemplate CreateItem( ILogger parameter ) => parameter.Error;
	}

	public class Fatal : CategoryFactory
	{
		public static Fatal Instance { get; } = new Fatal();

		protected override LogTemplate CreateItem( ILogger parameter ) => parameter.Fatal;
	}
}
