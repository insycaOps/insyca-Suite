using Microsoft.XLANGs.BaseTypes;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace inSyca.foundation.integration.biztalk.functions
{
    [CustomFormatter(typeof(rawstringformatter))]
    [Serializable]
    public class rawstring
    {
        [XmlIgnore]
        private string source;


        [XmlIgnore]
        private string encodingname;


        public rawstring(string input, string encodingName)
        {
            if (null == input)
            {
                throw new ArgumentNullException("input");
            }

            if (null == encodingName)
            {
                throw new ArgumentNullException("encodingName");
            }

            this.source = input;
            this.encodingname = encodingName;
        }


        public rawstring(string input)
            : this(input, "utf-8")
        {
        }


        public rawstring()
        {
        }


        public byte[] ToByteArray()
        {
            Encoding enc;

            try
            {
                enc = Encoding.GetEncoding(this.encodingname);
            }
            catch (ArgumentException)
            {
                enc = Encoding.UTF8;
            }

            return enc.GetBytes(this.source);
        }


        public override string ToString()
        {
            return this.source;
        }
    }

    public class rawstringformatter : baseformatter
    {
        public override void Serialize(Stream serializationStream, object graph)
        {
            rawstring rs = (rawstring)graph;
            byte[] ba = rs.ToByteArray();
            serializationStream.Write(ba, 0, ba.Length);
        }


        public override object Deserialize(Stream serializationStream)
        {
            StreamReader sr = new StreamReader(serializationStream, true);
            string s = sr.ReadToEnd();
            return new rawstring(s);
        }
    }

    public abstract class baseformatter : IFormatter
    {
        public virtual SerializationBinder Binder
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }


        public virtual StreamingContext Context
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }


        public virtual ISurrogateSelector SurrogateSelector
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }


        public abstract void Serialize(Stream serializationStream, object graph);


        public abstract object Deserialize(Stream serializationStream);
    }

}
