using System;
using System.Xml;

namespace inSyca.foundation.integration.biztalk.functions
{
    [Serializable]
	public sealed class soapxmldocument
	{
         public static string ToXmlString(XmlDocument xmlDoc)
        {
			return xmlDoc.OuterXml;
        }
	}
}
