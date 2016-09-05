using PostSharp.Aspects;

namespace DragonSpark.Aspects
{
	public interface IAspectHub
	{
		bool Enabled { get; }

		void Register( IAspect aspect );
	}
}