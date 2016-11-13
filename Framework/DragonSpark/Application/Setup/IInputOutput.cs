using System.IO;

namespace DragonSpark.Application.Setup
{
	public interface IInputOutput
	{
		TextReader Reader { get; }
		TextWriter Writer { get; }
	}
}