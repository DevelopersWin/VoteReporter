using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	[MarkupExtensionReturnType( typeof(DirectoryInfo) )]
	public class DirectoryInfoExtension : MarkupExtension
	{
		public DirectoryInfoExtension( string path )
	    {
	        Path = path;
	    }

	    public string Path { get; set; }

	    public override object ProvideValue( IServiceProvider serviceProvider )
		{
			var item = System.IO.Path.IsPathRooted( Path ) ? Path : System.IO.Path.GetFullPath( Path );
			var result = !DesignerProperties.GetIsInDesignMode( new DependencyObject() ) ? Directory.CreateDirectory( item ) : null;
			return result;
		}
	}
}