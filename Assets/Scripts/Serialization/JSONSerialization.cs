using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft;


namespace MyData
{
	public class JSONWriter : Writer, IDisposable
	{
		private Newtonsoft.Json.JsonTextWriter writer;

		public JSONWriter(string filePath)
		{
			try
			{
				writer = new Newtonsoft.Json.JsonTextWriter(new System.IO.StreamWriter(filePath));
				writer.Indentation = 4;
				writer.IndentChar = ' ';

				writer.WriteStartObject();
			}
			catch (Exception e)
			{
				writer = null;
				throw new WriteException("Unable to open " + filePath + ": " + e.Message);
			}
		}

		public override void Bool(bool value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteValue(value);
		}
		public override void Byte(byte value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteValue(value);
		}
		public override void Int(int value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteValue(value);
		}
		public override void UInt(uint value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteValue(value);
		}
		public override void Float(float value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteValue(value);
		}
		public override void Double(double value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteValue(value);
		}
		public override void String(string value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteValue(value);
		}
		public override void Bytes(byte[] value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteValue(value);
		}
		public override void Structure(IReadWritable value, string name)
		{
			writer.WritePropertyName(name);
			writer.WriteStartObject();
			value.WriteData(this);
			writer.WriteEndObject();
		}
		
		/// <summary>
		/// Flushes all serialized data to the JSON file, then closes this writer.
		/// </summary>
		public void Dispose()
		{
			writer.WriteEndObject();
			writer.Close();
		}
	}

	public class JSONReader : Reader
	{
		private Newtonsoft.Json.Linq.JObject root;

		public JSONReader(string filePath)
		{
			try
			{
				root = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(filePath));
			}
			catch (Exception e)
			{
				root = null;
				throw new ReadException("Error reading " + filePath + ": " + e.Message);
			}
		}
		private JSONReader(Newtonsoft.Json.Linq.JObject _root) { root = _root; }

		public override bool Bool(string name)
		{
			return root.Value<bool>(name);
		}
		public override byte Byte(string name)
		{
			return root.Value<byte>(name);
		}
		public override int Int(string name)
		{
			return root.Value<int>(name);
		}
		public override uint UInt(string name)
		{
			return root.Value<uint>(name);
		}
		public override float Float(string name)
		{
			return root.Value<float>(name);
		}
		public override double Double(string name)
		{
			return root.Value<double>(name);
		}
		public override string String(string name)
		{
			return root.Value<string>(name);
		}
		public override byte[] Bytes(string name)
		{
			return root.Value<byte[]>(name);
		}
		public override void Structure(IReadWritable outValue, string name)
		{
			outValue.ReadData(new JSONReader(root.Value<Newtonsoft.Json.Linq.JObject>(name)));
		}
	}
}