using DragonSpark.Activation;
using DragonSpark.Activation.Location;
using DragonSpark.ComponentModel;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using Type = System.Type;

namespace DragonSpark.Windows.Legacy.Markup
{
	public class ServiceExtension : MarkupExtensionBase
	{
		public ServiceExtension() {}

		public ServiceExtension( Type serviceType )
		{
			ServiceType = serviceType;
		}

		[PostSharp.Patterns.Contracts.NotNull]
		public Type ServiceType { [return: PostSharp.Patterns.Contracts.NotNull]get; set; }

		[Service, PostSharp.Patterns.Contracts.NotNull, UsedImplicitly]
		public IActivator Activator { [return: PostSharp.Patterns.Contracts.NotNull] get; set; } = DefaultServices.Default;

		protected override object GetValue( MarkupServiceProvider serviceProvider )
		{
			var service = Activator.Get( ServiceType ) ?? DefaultValues.Default.Get( ServiceType );
			return service;
		}
	}
}