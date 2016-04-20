using DragonSpark.Activation;
using Serilog;

namespace DragonSpark.Diagnostics.Logger.Categories
{
	public abstract class CategoryFactory : FactoryBase<ILogger, Log> {}

	public class Debug : CategoryFactory
	{
		public static Debug Instance { get; } = new Debug();

		protected override Log CreateItem( ILogger parameter ) => parameter.Debug;
	}

	public class Information : CategoryFactory
	{
		public static Information Instance { get; } = new Information();

		protected override Log CreateItem( ILogger parameter ) => parameter.Information;
	}

	public class Warning : CategoryFactory
	{
		public static Warning Instance { get; } = new Warning();

		protected override Log CreateItem( ILogger parameter ) => parameter.Warning;
	}

	public class Error : CategoryFactory
	{
		public static Error Instance { get; } = new Error();

		protected override Log CreateItem( ILogger parameter ) => parameter.Error;
	}

	public class Fatal : CategoryFactory
	{
		public static Fatal Instance { get; } = new Fatal();

		protected override Log CreateItem( ILogger parameter ) => parameter.Fatal;
	}
}
