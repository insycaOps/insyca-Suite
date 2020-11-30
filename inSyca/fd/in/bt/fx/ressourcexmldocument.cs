using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace inSyca.foundation.integration.biztalk.functions
{
    [Serializable]
    public class resourcexmldocument : XmlDocument
    {
        public resourcexmldocument(Type assemblyType, string resourceName, queryvalues queryvalues)
        {
            try
            {
                Assembly callingAssembly = Assembly.GetAssembly(assemblyType);

                if (null == callingAssembly)
                {
                    throw new resourceexception("GetExecutingAssembly returned null");
                }

                Stream resourceStream = callingAssembly.GetManifestResourceStream(resourceName);

                Load(resourceStream);

                if (null == queryvalues)
                {
                    throw new resourceexception("queryvalues not initialized");
                }

                if (queryvalues.Keys.Count < 1)
                {
                    throw new resourceexception("queryvalues.Keys must have at least one value");
                }


                foreach (string querycondition in queryvalues.Keys)
                {
                    XmlNode conditionNode = this.SelectSingleNode(querycondition);

                    if (null == conditionNode)
                    {
                        throw new resourceexception(string.Format(CultureInfo.InvariantCulture, "Condition: '{0}' did not return a XmlNode", querycondition));
                    }

                    XmlAttribute valueAttribute = conditionNode.Attributes["value"];

                    if (null == valueAttribute)
                    {
                        throw new resourceexception(string.Format(CultureInfo.InvariantCulture, "Condition: '{0}' with attribute 'value' did not return an XmlAttribute ", querycondition));
                    }

                    valueAttribute.Value = queryvalues[querycondition];
                }
            }
            catch (Exception ex)
            {
                throw new resourceexception(ex.Message);
            }
        }
    }

    [Serializable]
    public class resourceexception : Exception
    {
        public resourceexception()
            : base()
        {
        }


        public resourceexception(string message)
            : base(message)
        {
        }


        public resourceexception(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        protected resourceexception(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class queryvalues : Dictionary<string, string>
    {
        public queryvalues()
        {
        }


        protected queryvalues(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
