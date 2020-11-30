/// Pipeline component Built by Dipesh Avlani (http://integrationexperts.wordpress.com). Free to use in your projects.
///
/// <summary>
/// The purpose of this custom pipeline is to decode the xlsx files and convert it to xml. This component also adds a namespace.
/// A regular expressions are used to ensure blank cells are handled in the excel 2007 format.     
/// NamespaceBase:      -   The new namespace to be inserted. 
/// NamespacePrefix:    -   The namespace prefix to use.
/// RootNodeName:       -   Name of the root node.
/// IsFirstRowHeader:   -   Flag to indicate if the first row represents column names.
/// 
/// Note:   
/// This solution is based on the VirtualStream supported in BizTalk Server 2006 and up. The caching is set to to disk. The default location is for this file is 'C:\Documents and Settings\BTSHostInstanceName\Local Settings\Temp'. For performance reasons this location should be moved to a non OS-Drive. Make sure that BizTalk Host Instance account has full control of this folder. 
/// </summary>

using inSyca.foundation.framework;
using inSyca.foundation.integration.biztalk.components.diagnostics;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;

namespace inSyca.foundation.integration.biztalk.components
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Decoder)]
    [System.Runtime.InteropServices.Guid("dd0af82f-01dc-4aa1-9d0f-c613aa323c69")]
    public class Excel2007Decoder : IBaseComponent, IComponent, IPersistPropertyBag, IComponentUI
    {
        static private readonly ResourceManager _resourceManager = new ResourceManager("inSyca.foundation.integration.biztalk.components.Properties.Resources", Assembly.GetExecutingAssembly());

        static string NamespaceBaseNameLabel = "NamespaceBase";
        static string RootNodeNameLabel = "RootNodeName";
        static string NamespacePrefixNameLabel = "NamespacePrefix";
        static string IsFirstRowHeaderNameLabel = "IsFirstRowHeader";
        static string TempFolderNameLabel = "TempFolder";

        #region Properties

        [Description("NamespaceBase")]
        [DisplayName("NamespaceBase")]
        [DefaultValue("")]
        public string NamespaceBase { get; set; }

        [Description("NamespacePrefix")]
        [DisplayName("NamespacePrefix")]
        [DefaultValue("")]
        public string NamespacePrefix { get; set; }

        [Description("RootNodeName")]
        [DisplayName("RootNodeName")]
        [DefaultValue("")]
        public string RootNodeName { get; set; }

        [Description("IsFirstRowHeader")]
        [DisplayName("IsFirstRowHeader")]
        [DefaultValue(true)]
        public bool IsFirstRowHeader { get; set; }

        [Description("TempFolder")]
        [DisplayName("TempFolder")]
        [DefaultValue("")]
        public string TempFolder { get; set; }

        #endregion

        #region IBaseComponent Members

        /// <summary>
        /// Description of the component
        /// </summary>
        [Browsable(false)]
        public string Description
        {
            get
            {
                return _resourceManager.GetString("excelDecoder_description", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Name of the component
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return _resourceManager.GetString("excelDecoder_name", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Version of the component
        /// </summary>
        [Browsable(false)]
        public string Version
        {
            get
            {
                return _resourceManager.GetString("excelDecoder_version", CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region IPersistPropertyBag Members

        /// <summary>
        /// Gets class ID of component for usage from unmanaged code.
        /// </summary>
        /// <param name="classID">
        /// Class ID of the component
        /// </param>
        public void GetClassID(out Guid classID)
        {
            classID = new Guid("dd0af82f-08dc-4aa1-9d0f-c613aa323c69");

            Log.DebugFormat("GetClassID(out Guid {0})", classID);
        }

        /// <summary>
        /// not implemented
        /// </summary>
        public void InitNew()
        {
            Log.Debug("InitNew()");
        }

        /// <summary>
        /// Loads configuration properties for the component
        /// </summary>
        /// <param name="propertyBag">Configuration property bag</param>
        /// <param name="errorLog">Error status</param>
        public virtual void Load(IPropertyBag propertyBag, int errorLog)
        {
            Log.DebugFormat("Load(IPropertyBag propertyBag {0} , int errorLog {1})", propertyBag, errorLog);

            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;

                val = PropertyHelper.ReadPropertyBag(propertyBag, NamespaceBaseNameLabel);
                if (val != null)
                    NamespaceBase = ((string)(val));

                val = PropertyHelper.ReadPropertyBag(propertyBag, RootNodeNameLabel);
                if (val != null)
                    RootNodeName = ((string)(val));

                val = PropertyHelper.ReadPropertyBag(propertyBag, NamespacePrefixNameLabel);
                if (val != null)
                    NamespacePrefix = ((string)(val));

                val = PropertyHelper.ReadPropertyBag(propertyBag, IsFirstRowHeaderNameLabel);
                if (val != null)
                    IsFirstRowHeader = ((bool)(val));

                val = PropertyHelper.ReadPropertyBag(propertyBag, TempFolderNameLabel);
                if (val != null)
                    TempFolder = ((string)(val));
            }

            Log.DebugFormat("Load NamespaceBase {0}", NamespaceBase);
            Log.DebugFormat("Load RootNodeName {0}", RootNodeName);
            Log.DebugFormat("Load NamespacePrefix {0}", NamespacePrefix);
            Log.DebugFormat("Load IsFirstRowHeader {0}", IsFirstRowHeader);
            Log.DebugFormat("Load TempFolder {0}", TempFolder);
        }

        /// <summary>
        /// Saves the current component configuration into the property bag
        /// </summary>
        /// <param name="propertyBag">Configuration property bag</param>
        /// <param name="clearDirty">not used</param>
        /// <param name="saveAllProperties">not used</param>
        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            Log.DebugFormat("Save(IPropertyBag propertyBag {0}, bool clearDirty {1}, bool saveAllProperties {2})", propertyBag, clearDirty, saveAllProperties);

            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;

                val = NamespaceBase;
                propertyBag.Write(NamespaceBaseNameLabel, val);

                val = NamespacePrefix;
                propertyBag.Write(NamespacePrefixNameLabel, val);

                val = RootNodeName;
                propertyBag.Write(RootNodeNameLabel, val);

                val = IsFirstRowHeader;
                propertyBag.Write(IsFirstRowHeaderNameLabel, val);

                val = TempFolder;
                propertyBag.Write(TempFolderNameLabel, val);
            }

            Log.DebugFormat("Save NamespaceBase {0}", NamespaceBase);
            Log.DebugFormat("Save RootNodeName {0}", RootNodeName);
            Log.DebugFormat("Save NamespacePrefix {0}", NamespacePrefix);
            Log.DebugFormat("Save IsFirstRowHeader {0}", IsFirstRowHeader);
            Log.DebugFormat("Save TempFolder {0}", TempFolder);
        }

        #endregion

        #region IComponentUI Members

        /// <summary>
        /// Component icon to use in BizTalk Editor
        /// </summary>
        public IntPtr Icon
        {
            get
            {
                return Properties.Resources.cog.Handle;
            }
        }

        /// <summary>
        /// The Validate method is called by the BizTalk Editor during the build 
        /// of a BizTalk project.
        /// </summary>
        /// <param name="obj">An Object containing the configuration properties.</param>
        /// <returns>The IEnumerator enables the caller to enumerate through a collection of strings containing error messages. These error messages appear as compiler error messages. To report successful property validation, the method should return an empty enumerator.</returns>
        public IEnumerator Validate(object obj)
        {
            // example implementation:
            // ArrayList errorList = new ArrayList();
            // errorList.Add("This is a compiler error");
            // return errorList.GetEnumerator();
            return null;
        }
        #endregion

        #region IComponent Members

        /// <summary>
        /// Implements IComponent.Execute method.
        /// </summary>
        /// <param name="pipelineContext">Pipeline context</param>
        /// <param name="inMsg">Input message</param>
        /// <returns>Original input message</returns>
        /// <remarks>
        /// IComponent.Execute method is used to initiate
        /// the processing of the message in this pipeline component.
        /// </remarks>
        public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage inMsg)
        {
            Log.DebugFormat("Execute(IPipelineContext pipelineContext {0}, IBaseMessage inMsg {1})", pipelineContext, inMsg);

            if (string.IsNullOrEmpty(TempFolder))
                TempFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (string.IsNullOrEmpty(NamespaceBase))
                NamespaceBase = "http://insyca.bt.gl.eeb.schemas.integration.components.exceldecoder_10";

            if (string.IsNullOrEmpty(RootNodeName))
                RootNodeName = "exceldecoder";

            if (string.IsNullOrEmpty(NamespacePrefix))
                RootNodeName = "ex0";

            var outMsg = pipelineContext.GetMessageFactory().CreateMessage();

            try
            {
                if (inMsg == null || inMsg.BodyPart == null || inMsg.BodyPart.Data == null)
                {
                    Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, "pInMsg Error", null));
                    throw new ArgumentNullException("pInMsg");
                }

                var fileAdapterTargetNamespace = "http://schemas.microsoft.com/BizTalk/2003/file-properties";
                var bodyPart = inMsg.BodyPart;

                var inputFileName = Convert.ToString(inMsg.Context.Read("ReceivedFileName", fileAdapterTargetNamespace));

                var inboundStream = bodyPart.GetOriginalDataStream();

                MemoryStream stream = ConvertExceltoXML(inboundStream, inputFileName);

                stream.Seek(0, SeekOrigin.Begin);
                outMsg.AddPart("Body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
                outMsg.BodyPart.Data = stream;

                //Promote properties if required.
                for (int iProp = 0; iProp < inMsg.Context.CountProperties; iProp++)
                {
                    string strName;
                    string strNSpace;
                    object val = inMsg.Context.ReadAt(iProp, out strName, out strNSpace);

                    // If the property has been promoted, respect the settings
                    if (inMsg.Context.IsPromoted(strName, strNSpace))
                        outMsg.Context.Promote(strName, strNSpace, val);
                    else
                        outMsg.Context.Write(strName, strNSpace, val);

                    //update the ReceivedFileName with the actual file entry name
                    if (strName == "ReceivedFileName")
                    {
                        outMsg.Context.Write(strName, strNSpace, inputFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, ex));
            }

            return outMsg;
        }

        #endregion

        #region Helper

        private MemoryStream ConvertExceltoXML(Stream excelStream, string inputFileName)
        {
            Log.DebugFormat("ConvertExceltoXML(Stream excelStream {0}, string inputFileName {1})", excelStream, inputFileName);

            var tempDir = string.Format("{0}\\{1}", TempFolder, Guid.NewGuid());
            string fileName = string.Format("{0}\\{1}", tempDir, Path.GetFileName(inputFileName));
            string HDR = IsFirstRowHeader ? "Yes" : "No";
            string strConn;

            try
            {
                Log.DebugFormat("ConvertExceltoXML(Stream excelStream {0}, string inputFileName {1})\ntempDir: {2}", excelStream, inputFileName, tempDir);

                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                Log.DebugFormat("ConvertExceltoXML(Stream excelStream {0}, string inputFileName {1})\nfileName: {2}", excelStream, inputFileName, fileName);

                using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    excelStream.CopyTo(file);
                    using (StreamWriter s = new StreamWriter(file))
                        s.Flush();
                }

                if (fileName.Substring(fileName.LastIndexOf('.')).ToLower() == ".xlsx")
                    strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=" + HDR + ";IMEX=0'";
                else
                    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties='Excel 8.0;HDR=" + HDR + ";IMEX=0'";

                DataSet dataSet = new DataSet(RootNodeName);
                dataSet.Namespace = NamespaceBase;
                dataSet.Prefix = NamespacePrefix;

                using (OleDbConnection conn = new OleDbConnection(strConn))
                {
                    conn.Open();

                    DataTable schemaTable = conn.GetOleDbSchemaTable(
                        OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                    foreach (DataRow schemaRow in schemaTable.Rows)
                    {
                        string sheet = schemaRow["TABLE_NAME"].ToString();

                        if (!sheet.EndsWith("_"))
                        {
                            try
                            {
                                OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                                cmd.CommandType = CommandType.Text;

                                DataTable outputTable = new DataTable(sheet);
                                outputTable.Namespace = NamespaceBase;
                                outputTable.Prefix = NamespacePrefix;
                                dataSet.Tables.Add(outputTable);
                                new OleDbDataAdapter(cmd).Fill(outputTable);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(new LogEntry(MethodBase.GetCurrentMethod(), null, ex));

                                throw new Exception(ex.Message + string.Format("Sheet:{0}.File:F{1}", sheet, fileName), ex);
                            }
                        }
                    }
                }

                var stream = new MemoryStream();
                dataSet.WriteXml(stream);
                return stream;
            }
            catch (Exception ex)
            {
                diagnostics.Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, ex));
                throw ex;
            }
            finally
            {
                //// Clean up
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        public static Stream ConvertExceltoXML(Stream inputFileStream, bool hasHeaders, bool autoDetectColumnType)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(new XmlTextReader(inputFileStream));
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

            nsmgr.AddNamespace("o", "urn:schemas-microsoft-com:office:office");
            nsmgr.AddNamespace("x", "urn:schemas-microsoft-com:office:excel");
            nsmgr.AddNamespace("ss", "urn:schemas-microsoft-com:office:spreadsheet");

            DataSet dataSet = new DataSet();

            foreach (XmlNode node in
              doc.DocumentElement.SelectNodes("//ss:Worksheet", nsmgr))
            {
                DataTable dt = new DataTable(node.Attributes["ss:Name"].Value);
                dataSet.Tables.Add(dt);
                XmlNodeList rows = node.SelectNodes("ss:Table/ss:Row", nsmgr);
                if (rows.Count > 0)
                {

                    //*************************
                    //Add Columns To Table from header row
                    //*************************
                    List<ColumnType> columns = new List<ColumnType>();
                    int startIndex = 0;
                    if (hasHeaders)
                    {
                        foreach (XmlNode data in rows[0].SelectNodes("ss:Cell/ss:Data", nsmgr))
                        {
                            columns.Add(new ColumnType(typeof(string)));//default to text
                            dt.Columns.Add(data.InnerText, typeof(string));
                        }
                        startIndex++;
                    }
                    //*************************
                    //Update Data-Types of columns if Auto-Detecting
                    //*************************
                    if (autoDetectColumnType && rows.Count > 0)
                    {
                        XmlNodeList cells = rows[startIndex].SelectNodes("ss:Cell", nsmgr);
                        int actualCellIndex = 0;
                        for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
                        {
                            XmlNode cell = cells[cellIndex];
                            if (cell.Attributes["ss:Index"] != null)
                                actualCellIndex =
                                  int.Parse(cell.Attributes["ss:Index"].Value) - 1;

                            ColumnType autoDetectType =
                              getType(cell.SelectSingleNode("ss:Data", nsmgr));

                            if (actualCellIndex >= dt.Columns.Count)
                            {
                                dt.Columns.Add("Column" +
                                  actualCellIndex.ToString(), autoDetectType.type);
                                columns.Add(autoDetectType);
                            }
                            else
                            {
                                dt.Columns[actualCellIndex].DataType = autoDetectType.type;
                                columns[actualCellIndex] = autoDetectType;
                            }

                            actualCellIndex++;
                        }
                    }
                    //*************************
                    //Load Data
                    //*************************
                    for (int i = startIndex; i < rows.Count; i++)
                    {
                        DataRow row = dt.NewRow();
                        XmlNodeList cells = rows[i].SelectNodes("ss:Cell", nsmgr);
                        int actualCellIndex = 0;
                        for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
                        {
                            XmlNode cell = cells[cellIndex];
                            if (cell.Attributes["ss:Index"] != null)
                                actualCellIndex = int.Parse(cell.Attributes["ss:Index"].Value) - 1;

                            XmlNode data = cell.SelectSingleNode("ss:Data", nsmgr);

                            if (actualCellIndex >= dt.Columns.Count)
                            {
                                for (int j = dt.Columns.Count; j < actualCellIndex; j++)
                                {
                                    dt.Columns.Add("Column" +
                                               actualCellIndex.ToString(), typeof(string));
                                    columns.Add(getDefaultType());
                                }
                                ColumnType autoDetectType =
                                   getType(cell.SelectSingleNode("ss:Data", nsmgr));
                                dt.Columns.Add("Column" + actualCellIndex.ToString(),
                                               typeof(string));
                                columns.Add(autoDetectType);
                            }
                            if (data != null)
                                row[actualCellIndex] = data.InnerText;

                            actualCellIndex++;
                        }

                        dt.Rows.Add(row);
                    }
                }
            }

            var stream = new MemoryStream();
            dataSet.WriteXml(stream);
            dataSet.WriteXml(@"c:\temp\excel.xml");
            return stream;
        }

        struct ColumnType
        {
            public Type type;
            private string name;
            public ColumnType(Type type) { this.type = type; this.name = type.ToString().ToLower(); }
            public object ParseString(string input)
            {
                if (String.IsNullOrEmpty(input))
                    return DBNull.Value;
                switch (type.ToString())
                {
                    case "system.datetime":
                        return DateTime.Parse(input);
                    case "system.decimal":
                        return decimal.Parse(input);
                    case "system.boolean":
                        return bool.Parse(input);
                    default:
                        return input;
                }
            }
        }

        private static ColumnType getDefaultType()
        {
            return new ColumnType(typeof(String));
        }

        private static ColumnType getType(XmlNode data)
        {
            string type = null;
            if (data.Attributes["ss:Type"] == null || data.Attributes["ss:Type"].Value == null)
                type = "";
            else
                type = data.Attributes["ss:Type"].Value;

            switch (type)
            {
                case "DateTime":
                    return new ColumnType(typeof(DateTime));
                case "Boolean":
                    return new ColumnType(typeof(Boolean));
                case "Number":
                    return new ColumnType(typeof(Decimal));
                case "":
                    decimal test2;
                    if (data == null || String.IsNullOrEmpty(data.InnerText) || decimal.TryParse(data.InnerText, out test2))
                    {
                        return new ColumnType(typeof(Decimal));
                    }
                    else
                    {
                        return new ColumnType(typeof(String));
                    }
                default://"String"
                    return new ColumnType(typeof(String));
            }
        }
        #endregion
    }
}
