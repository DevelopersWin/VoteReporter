using DragonSpark.Commands;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DragonSpark.Application.Setup
{
	public class ApplyExportCommands<T> : DisposingCommand<object> where T : class, ICommand
	{
		[Required, Service]
		public IExportProvider Exports { [return: Required]get; set; }

		public string ContractName { get; set; }

		readonly ICollection<T> watching = new WeakList<T>();

		public override void Execute( object parameter )
		{
			var exports = Exports.GetExports<T>( ContractName );
			watching.AddRange( exports.AsEnumerable() );

			foreach ( var export in exports )
			{
				export.Execute( parameter );
			}
		}

		protected override void OnDispose()
		{
			watching.Purge().OfType<IDisposable>().Each( obj => obj.Dispose() );
			base.OnDispose();
		}
	}
}