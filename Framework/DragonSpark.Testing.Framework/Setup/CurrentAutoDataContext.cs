using DragonSpark.Runtime.Values;

namespace DragonSpark.Testing.Framework.Setup
{
	public class CurrentAutoDataContext : ExecutionContextValue<AutoData>
	{
		protected override void OnDispose()
		{
			base.OnDispose();
			Item.Dispose();
		}
	}
}