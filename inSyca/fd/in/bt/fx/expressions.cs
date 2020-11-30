using inSyca.foundation.integration.biztalk.functions.diagnostics;
using Microsoft.BizTalk.Component.Interop;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace inSyca.foundation.integration.biztalk.functions
{
    public class expressions
    {
        static public XmlDocument GenerateExcelDocument(XmlDocument message)
        {
            XmlElement xmlColumn;
            XmlElement xmlRow;
            XmlElement xmlCell;
            XmlElement xmlData;
            XmlDocument excelXmlDocument = new XmlDocument();
            XmlNode root = message.DocumentElement;
            XmlNode firstChild = root.FirstChild;

            excelXmlDocument.LoadXml(@"<ns0:Workbook xmlns:ns0='urn:schemas-microsoft-com:office:spreadsheet'>
                                              <ns0:Styles>
                                                <ns0:Style ns0:ID='1'>
                                                  <ns0:Font ns0:Bold='1' />
                                                </ns0:Style>
                                              </ns0:Styles>
                                              <ns0:Worksheet ns0:Name='Sheet1'>
                                                <ns0:Table>
                                                    <ns0:Column ns0:Width='80' />
                                                </ns0:Table>
                                              </ns0:Worksheet>
                                            </ns0:Workbook>");

            foreach (XmlElement columnNode in firstChild.ChildNodes)
            {
                xmlColumn = excelXmlDocument.CreateElement("ns0:Column", "urn:schemas-microsoft-com:office:spreadsheet");
                xmlColumn.SetAttribute("Width", "urn:schemas-microsoft-com:office:spreadsheet", "80");
                excelXmlDocument.DocumentElement.LastChild.FirstChild.AppendChild(xmlColumn);
            }

            xmlRow = excelXmlDocument.CreateElement("ns0:Row", "urn:schemas-microsoft-com:office:spreadsheet");
            xmlRow.SetAttribute("StyleID", "urn:schemas-microsoft-com:office:spreadsheet", "1");

            foreach (XmlElement columnNode in firstChild.ChildNodes)
            {
                xmlCell = excelXmlDocument.CreateElement("ns0:Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                xmlData = excelXmlDocument.CreateElement("ns0:Data", "urn:schemas-microsoft-com:office:spreadsheet");
                xmlData.InnerText = columnNode.Name;

                xmlData.SetAttribute("Type", "urn:schemas-microsoft-com:office:spreadsheet", "String");
                xmlCell.AppendChild(xmlData);
                xmlRow.AppendChild(xmlCell);
            }

            excelXmlDocument.DocumentElement.LastChild.FirstChild.AppendChild(xmlRow);

            foreach (XmlNode child in root.ChildNodes)
            {
                xmlRow = excelXmlDocument.CreateElement("ns0:Row", "urn:schemas-microsoft-com:office:spreadsheet");

                foreach (XmlNode element in child.ChildNodes)
                {
                    xmlCell = excelXmlDocument.CreateElement("ns0:Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                    xmlData = excelXmlDocument.CreateElement("ns0:Data", "urn:schemas-microsoft-com:office:spreadsheet");
                    xmlData.InnerText = element.InnerText;

                    xmlData.SetAttribute("Type", "urn:schemas-microsoft-com:office:spreadsheet", "String");
                    xmlCell.AppendChild(xmlData);
                    xmlRow.AppendChild(xmlCell);
                }

                excelXmlDocument.DocumentElement.LastChild.FirstChild.AppendChild(xmlRow);
            }

            return excelXmlDocument;
        }

        public static XmlDocument ConstructEmptyMessage(Type objType)
        {
            Type MessageSchemaType = objType;

            try
            {
                DocumentSpec DocumentSpecification = new DocumentSpec(
                  MessageSchemaType.FullName,
                  MessageSchemaType.Assembly.FullName
                  );

                XmlDocument SchemaInstance = new XmlDocument();
                using (StreamReader InstanceStreamReader = new StreamReader(DocumentSpecification.GetDocSchema().CreateXmlInstance()))
                {
                    SchemaInstance.Load(InstanceStreamReader);
                }

                return SchemaInstance;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static XmlDocument ConstructMessageWithData(Type objType)
        {
            Type MessageSchemaType = objType;

            try
            {
                DocumentSpec DocumentSpecification = new DocumentSpec(
                  MessageSchemaType.FullName,
                  MessageSchemaType.Assembly.FullName
                  );

                XmlDocument SchemaInstance = new XmlDocument();
                using (StreamReader InstanceStreamReader = new StreamReader(DocumentSpecification.GetDocSchema().CreateXmlInstanceWithData()))
                {
                    SchemaInstance.Load(InstanceStreamReader);
                }

                return SchemaInstance;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void ProcessBatchMessage(ref XmlDocument xmlBatch, int increment)
        {
            Log.DebugFormat("ProcessBatchMessage (ref XmlDocument xmlBatch {0} , int increment)", xmlBatch, increment);

            XElement xBatch = XElement.Parse(xmlBatch.OuterXml);
            XElement xIndex = xBatch.Elements("index").FirstOrDefault();

            int index = int.Parse(xIndex.Value);

            xIndex.Value = Convert.ToString(index + increment);

            xmlBatch.LoadXml(xBatch.ToString());
        }
    }
}
