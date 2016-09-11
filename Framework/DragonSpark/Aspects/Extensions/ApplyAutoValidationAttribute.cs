namespace DragonSpark.Aspects.Extensions
{
	public sealed class ApplyAutoValidationAttribute : ApplyExtensionsAttribute
	{
		public ApplyAutoValidationAttribute() : base( typeof(AutoValidationExtension) ) {}
	}
}