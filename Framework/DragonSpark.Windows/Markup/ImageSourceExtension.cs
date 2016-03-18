using DragonSpark.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

// Credit: http://blogs.msdn.com/b/ifeanyie/archive/2010/03/27/9986217.aspx
namespace DragonSpark.Windows.Markup
{
	[MarkupExtensionReturnType( typeof(ImageSource) )]
	public class ImageSourceExtension : MarkupExtensionBase
	{
		public ImageSourceExtension()
		{}

		public ImageSourceExtension( string sourceUri ) : this( new Uri( sourceUri ) )
		{}

		public ImageSourceExtension( Uri sourceUri ) : this()
		{
			Source = sourceUri;
		}

		public Uri Source { get; set; }

		public ImageSource BusySource { get; set; }

		public ImageSource ErrorSource { get; set; }

		object SetFrozenImageSourceFromUri( Uri uri, IMarkupProperty setter )
		{
			ImageSourceOperations.GetFrozenImageSourceFromUri( uri, 
				x =>
				{
					setter.SetValue( x );
					// setter.Dispose();
				}, 
				e =>
				{
					setter.SetValue( ErrorSource );
					// setter.Dispose();
				} );

			return null;
		}

		static Uri ResolveUri( DependencyObject dataContextSource, Uri baseUri, Uri sourceUri )
		{
			if ( sourceUri == null && dataContextSource != null )
			{
				sourceUri = dataContextSource.GetValue( FrameworkElement.DataContextProperty ) as Uri;
			}

			var result = baseUri != null && baseUri.IsAbsoluteUri && sourceUri != null ? new Uri( baseUri, sourceUri ) : sourceUri;
			return result;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider )
		{
			var baseUri = serviceProvider.Get<IUriContext>().With( context => context.BaseUri );
			
			var targetObject = serviceProvider.Get<IProvideValueTarget>().With( target => target.TargetObject as DependencyObject );
			
			var imageUri = ResolveUri( targetObject, baseUri, Source );

			if ( imageUri != null && imageUri.IsAbsoluteUri )
			{
				Task.Run( () => SetFrozenImageSourceFromUri( imageUri, serviceProvider.Property ) )/*.ConfigureAwait( false )*/;
			}

			return BusySource;
		}
	}
}
