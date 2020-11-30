///
///
///
///
///
///
///
///
///
///
///
///
///

using inSyca.foundation.framework;
using inSyca.foundation.integration.biztalk.components.diagnostics;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;


namespace inSyca.foundation.integration.biztalk.components
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
    [Guid("6118A9F0-8684-4ba2-87B4-8336D70BD4F7")]
    public class FFXmlSplitter : FFDasmComp, IBaseComponent, IDisassemblerComponent, IComponentUI, IPersistPropertyBag
    {
        static private readonly ResourceManager _resourceManager = new ResourceManager("inSyca.foundation.integration.biztalk.components.Properties.Resources", Assembly.GetExecutingAssembly());

        static string ChildNodeNameLabel = "ChildNodeName";
        static string GroupByNodeNameLabel = "GroupByNodeName";

        //Used to hold disassembled messages
        private System.Collections.Queue qOutputMsgs = null;
        private string systemPropertiesNamespace = @"http://schemas.microsoft.com/BizTalk/2003/system-properties";

        #region Properties
        [Description("Name of nodes to split")]
        [DisplayName("ChildNodeName")]
        [DefaultValue("")]
        public string ChildNodeName { get; set; }

        [Description("Name of nodes to groupby")]
        [DisplayName("GroupByNodeName")]
        [DefaultValue("")]
        public string GroupByNodeName { get; set; }

        #endregion

        #region IBaseComponent Members

        /// <summary>
        /// Description of the component
        /// </summary>
        [Browsable(false)]
        public new string Description
        {
            get
            {
                return _resourceManager.GetString("ffXmlSplitter_description", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Name of the component
        /// </summary>
        [Browsable(false)]
        public new string Name
        {
            get
            {
                return _resourceManager.GetString("ffXmlSplitter_name", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Version of the component
        /// </summary>
        [Browsable(false)]
        public new string Version
        {
            get
            {
                return _resourceManager.GetString("ffXmlSplitter_version", CultureInfo.InvariantCulture);
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
        public new void GetClassID(out Guid classID)
        {
            classID = new System.Guid("16D92957-2034-4853-840B-BD424C596247");

            Log.DebugFormat("GetClassID(out Guid {0})", classID);
        }

        /// <summary>
        /// not implemented
        /// </summary>
        public new void InitNew()
        {
            Log.Debug("InitNew()");
        }

        /// <summary>
        /// Loads configuration properties for the component
        /// </summary>
        /// <param name="propertyBag">Configuration property bag</param>
        /// <param name="errorLog">Error status</param>
		public new void Load(IPropertyBag propertyBag, int errorLog)
        {
            Log.DebugFormat("Load(IPropertyBag propertyBag {0} , int errorLog {1})", propertyBag, errorLog);

            base.Load(propertyBag, errorLog);

            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;

                val = PropertyHelper.ReadPropertyBag(propertyBag, ChildNodeNameLabel);

                if (val != null)
                    ChildNodeName = (string)val;

                val = PropertyHelper.ReadPropertyBag(propertyBag, GroupByNodeNameLabel);

                if (val != null)
                    GroupByNodeName = (string)val;
            }

            Log.DebugFormat("Load ChildNodeName {0}", ChildNodeName);
            Log.DebugFormat("Load GroupByNodeName {0}", GroupByNodeName);
        }

        /// <summary>
        /// Saves the current component configuration into the property bag
        /// </summary>
        /// <param name="propertyBag">Configuration property bag</param>
        /// <param name="clearDirty">not used</param>
        /// <param name="saveAllProperties">not used</param>
        public new void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            base.Save(propertyBag, clearDirty, saveAllProperties);

            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;

                val = ChildNodeName;
                propertyBag.Write(ChildNodeNameLabel, ref val);

                val = GroupByNodeName;
                propertyBag.Write(GroupByNodeNameLabel, ref val);
            }

            Log.DebugFormat("Save ChildNodeName {0}", ChildNodeName);
            Log.DebugFormat("Save GroupByNodeName {0}", GroupByNodeName);
        }

        #endregion

        #region IComponentUI members
        /// <summary>
        /// Component icon to use in BizTalk Editor
        /// </summary>
        public new IntPtr Icon
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
        public new IEnumerator Validate(object obj)
        {
            // example implementation:
            // ArrayList errorList = new ArrayList();
            // errorList.Add("This is a compiler error");
            // return errorList.GetEnumerator();
            return null;
        }
        #endregion

        #region IDisassemblerComponent
        /// <summary>
        /// Disassembles (breaks) message into small messages as per batch size
        /// </summary>
        public new void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            Log.DebugFormat("Disassemble(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})", pContext, pInMsg);

            base.DocumentSpecName = this.DocumentSpecName;
            base.Disassemble(pContext, pInMsg);
        }

        /// <summary>
        /// Used to pass output messages`to next stage
        /// </summary>
        public new IBaseMessage GetNext(IPipelineContext pContext)
        {
            if(qOutputMsgs == null)
            {
                qOutputMsgs = new System.Collections.Queue();

                IBaseMessage pBaseMessage = base.GetNext(pContext);

                SplitXmlMessage(pContext, pBaseMessage);

                if (qOutputMsgs.Count == 0)
                    return null;
            }

            if (qOutputMsgs.Count > 0)
            {
                Log.DebugFormat("_msgs.Count {0}", qOutputMsgs.Count);

                return ((IBaseMessage)(qOutputMsgs.Dequeue()));
            }
            else
            {
                Log.DebugFormat("Queue empty");
                return null;
            }
        }


        #endregion

        #region Helper
        private void SplitXmlMessage(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            XmlReader xmlReader = null;

            try
            {
                xmlReader = new XmlTextReader(pInMsg.BodyPart.GetOriginalDataStream());
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { pContext, pInMsg }, ex));
            }

            try
            {
                XElement xml = XElement.Load(xmlReader);

                string namespaceURI, rootElement;
                System.Collections.Generic.IEnumerable<IGrouping<string, XElement>> childNodes;

                ExtractChildNodes(xml, out namespaceURI, out rootElement, out childNodes);

                Log.DebugFormat("Disassemble(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})\nnamespaceURI: {2}, rootElement: {3}, ChildNodeName: {4}, GroupByNodeName: {5}", pContext, pInMsg, namespaceURI, rootElement, ChildNodeName, GroupByNodeName);

                XElement xmlTemp = new XElement(xml);

                foreach (var childNodeGroup in childNodes)
                {
                    Log.DebugFormat("Disassemble(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})\nKey: {2}", pContext, pInMsg, childNodeGroup.Key);

                    xmlTemp.RemoveNodes();

                    // Nested foreach is required to access group items. 
                    foreach (var childNode in childNodeGroup)
                        xmlTemp.Add(childNode);

                    CreateOutgoingMessage(pInMsg, pContext, xmlTemp.ToString(), namespaceURI, rootElement);

                    Log.DebugFormat("Disassemble(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})\nxmlTemp: {2}", pContext, pInMsg, xmlTemp.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { pContext, pInMsg }, ex));
            }
            finally
            {
                xmlReader = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="namespaceURI"></param>
        /// <param name="rootElement"></param>
        /// <param name="childNodes"></param>
        public void ExtractChildNodes(XElement xml, out string namespaceURI, out string rootElement, out System.Collections.Generic.IEnumerable<IGrouping<string, XElement>> childNodes)
        {
            //fetch namespace and root element
            namespaceURI = xml.Name.NamespaceName;
            rootElement = xml.Name.LocalName;

            XNamespace xmlns = xml.GetDefaultNamespace();

            XName childNodeName;
            XName groupByNodeName;

            if (string.IsNullOrEmpty(xmlns.NamespaceName))
            {
                childNodeName = ChildNodeName;
                groupByNodeName = GroupByNodeName;
            }
            else
            {
                childNodeName = xmlns + ChildNodeName;
                groupByNodeName = xmlns + GroupByNodeName;
            }
            childNodes = from s in xml.Descendants(childNodeName)
                         group s by (string)s.Element(groupByNodeName);
        }

        /// <summary>
        /// Queue outgoing messages
        /// </summary>
        private void CreateOutgoingMessage(IBaseMessage pInMsg, IPipelineContext pContext, String messageString, string namespaceURI, string rootElement)
        {
            Log.DebugFormat("CreateOutgoingMessage(IBaseMessage pInMsg {0}, IPipelineContext pContext{1}, String messageString {2}, string namespaceURI {3}, string rootElement {4})", pInMsg, pContext, messageString, namespaceURI, rootElement);

            IBaseMessage outMsg;

            try
            {
                //create outgoing message
                outMsg = pContext.GetMessageFactory().CreateMessage();
                outMsg.Context = pInMsg.Context;
                outMsg.AddPart("Body", pContext.GetMessageFactory().CreateMessagePart(), true);
                outMsg.Context.Promote("MessageType", systemPropertiesNamespace, namespaceURI + "#" + rootElement.Replace("ns0:", ""));
                byte[] bufferOutgoingMessage = System.Text.UTF8Encoding.UTF8.GetBytes(messageString);
                outMsg.BodyPart.Data = new MemoryStream(bufferOutgoingMessage);
                qOutputMsgs.Enqueue(outMsg);
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { pInMsg, pContext, messageString, namespaceURI, rootElement }, ex));
            }
        }
        #endregion
    }
}
