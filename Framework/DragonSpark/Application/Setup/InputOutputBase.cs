using System.IO;

namespace DragonSpark.Application.Setup
{
	public abstract class InputOutputBase : IInputOutput
	{
		protected InputOutputBase( TextWriter writer, TextReader reader )
		{
			Writer = writer;
			Reader = reader;
		}

		public TextWriter Writer { get; }
		public TextReader Reader { get; }
	}
}