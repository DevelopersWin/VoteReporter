using System.Runtime.InteropServices;

namespace DragonSpark.Aspects.Validation
{
	public static class Extensions
	{
		public static bool Marked( this IAutoValidationController @this, [Optional]object parameter, bool valid )
		{
			@this.MarkValid( parameter, valid );
			return valid;
		}
	}
}