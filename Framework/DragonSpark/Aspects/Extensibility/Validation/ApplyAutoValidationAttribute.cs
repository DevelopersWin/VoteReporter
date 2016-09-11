using DragonSpark.Extensions;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public sealed class ApplyAutoValidationAttribute : ApplyExtensionsAttribute
	{
		readonly static AutoValidationExtension[] Extension = AutoValidationExtension.Default.ToItem();

		public ApplyAutoValidationAttribute() : base( Extension ) {}
	}
}