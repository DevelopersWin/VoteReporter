using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition;
using System.IO;
using System.Xaml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace DragonSpark.Windows.Runtime.Data
{
	public sealed class Serializer : ISerializer
	{
		[Export]
		public static ISerializer Default { get; } = new Serializer();
		Serializer() {}

		public T Load<T>( Stream data ) => (T)XamlServices.Load( data );

		public string Save<T>( T item ) => XamlServices.Save( item );
	}

	public abstract class DataTransformerBase<T> : ParameterizedSourceBase<DataTransformParameter, T> {}

	public abstract class DataTransformer<T> : DataTransformerBase<T>
	{
		readonly Func<DataTransformParameter, MemoryStream> factory;
		readonly Func<MemoryStream, T> transformer;

		protected DataTransformer( Func<MemoryStream, T> transformer ) : this( DataStreamFactory.Default.Get, transformer ) {}

		protected DataTransformer( [Required]Func<DataTransformParameter, MemoryStream> factory, [Required]Func<MemoryStream, T> transformer )
		{
			this.factory = factory;
			this.transformer = transformer;
		}

		public override T Get( DataTransformParameter parameter )
		{
			var stream = factory( parameter );
			var result = transformer( stream );
			return result;
		}
	}

	public sealed class DataTransformer : DataTransformer<string>
	{
		public static DataTransformer Default { get; } = new DataTransformer();
		DataTransformer() : base( stream => new StreamReader( stream ).ReadToEnd() ) {}
	}

	public class DataSerializer : DataSerializer<object>
	{
		public new static DataSerializer Default { get; } = new DataSerializer();
		DataSerializer() {}
	}

	public class DataSerializer<T> : DataTransformer<T>
	{
		public static DataSerializer<T> Default { get; } = new DataSerializer<T>();
		protected DataSerializer() : this( Serializer.Default ) {}

		public DataSerializer( ISerializer serializer ) : base( serializer.Load<T> ) {}
	}

	public class DataStreamFactory : DataTransformerBase<MemoryStream>
	{
		public static DataStreamFactory Default { get; } = new DataStreamFactory();

		public override MemoryStream Get( DataTransformParameter parameter )
		{
			var transform = new XslCompiledTransform();
			transform.Load( parameter.Stylesheet );

			var stream = new MemoryStream();
			transform.Transform( parameter.Source, null, stream );
			stream.Seek( 0, SeekOrigin.Begin );
			return stream;
		}
	}

	public struct DataTransformParameter
	{
		public DataTransformParameter( IXPathNavigable stylesheet, IXPathNavigable source )
		{
			Stylesheet = stylesheet;
			Source = source;
		}

		public IXPathNavigable Source { get; }

		public IXPathNavigable Stylesheet { get; }
	}
}