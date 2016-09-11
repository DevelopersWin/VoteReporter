namespace DragonSpark.Aspects.Extensibility.Validation
{
	public sealed class ApplyAutoValidationAttribute : ApplyExtensionsAttribute
	{
		public ApplyAutoValidationAttribute() : base( typeof(AutoValidationExtension) ) {}
	}
}