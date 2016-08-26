using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Composition
{
	public class ExportMapping<TSubject, TExport> : ExportMapping where TSubject : TExport
	{
		public ExportMapping() : base( typeof(TSubject), typeof(TExport) ) {}
	}

	public class ExportMapping
	{
		public ExportMapping() {}

		public ExportMapping( Type subject, Type exportAs )
		{
			Subject = subject;
			ExportAs = exportAs;
		}

		public Type Subject { [return: NotNull]get; set; }

		public Type ExportAs { get; set; }
	}

}