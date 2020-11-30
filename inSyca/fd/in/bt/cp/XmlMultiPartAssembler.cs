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
using System.Reflection;
using System.Resources;
using System.Xml;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;

namespace inSyca.foundation.integration.biztalk.components
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    [ComponentCategory(CategoryTypes.CATID_Decoder)]
    [ComponentCategory(CategoryTypes.CATID_Encoder)]
    [ComponentCategory(CategoryTypes.CATID_Validate)]
    [System.Runtime.InteropServices.Guid("48BEC85A-20EE-40ad-BFD0-319B59A0DDBD")]
    public class XmlMultiPartAssembler : IBaseComponent, IComponent, IPersistPropertyBag, IComponentUI
    {
        static private readonly ResourceManager _resourceManager = new ResourceManager("inSyca.foundation.integration.biztalk.components.Properties.Resources", Assembly.GetExecutingAssembly());

        static string RootNodeNameLabel = "RootNodeName";

        #region Properties
        /// <summary>
        /// Location of Xsl transform file.
        /// </summary>
        [Description("RootNodeName")]
        [DisplayName("RootNodeName")]
        [DefaultValue("")]
        public string RootNodeName { get; set; }
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
                return _resourceManager.GetString("xmlMultiPartAssembler_description", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("xmlMultiPartAssembler_name", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("xmlMultiPartAssembler_version", CultureInfo.InvariantCulture);
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
            classID = new System.Guid("48BEC85A-20EE-40ad-BFD0-319B59A0DDBD");

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
        public void Load(IPropertyBag propertyBag, int errorLog)
        {
            Log.DebugFormat("Load(IPropertyBag propertyBag {0} , int errorLog {1})", propertyBag, errorLog);

            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;

                val = PropertyHelper.ReadPropertyBag(propertyBag, RootNodeNameLabel);

                if (val != null)
                    RootNodeName = (string)val;
            }
            Log.DebugFormat("Load RootNodeName {0}", RootNodeName);
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

                val = RootNodeName;
                propertyBag.Write(RootNodeNameLabel, ref val);
            }

            Log.DebugFormat("Save RootNodeName {0}", RootNodeName);
        }

        #endregion

        #region IComponentUI members
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

        #region IProbeMessage Members

        public bool Probe(IPipelineContext pipelineContext, IBaseMessage inMsg)
        {
            return null != inMsg;
        }

        #endregion

        #region IComponent Members

        public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage inMsg)
        {
            Log.DebugFormat("Execute(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})", pipelineContext, inMsg);

            try
            {
                inMsg.BodyPart.Data = AssembleMessage(inMsg);
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { pipelineContext, inMsg }, ex));
            }

            return inMsg;
        }

        private Stream AssembleMessage(IBaseMessage inMsg)
        {
            Log.DebugFormat("AssembleMessage(IBaseMessage inMsg {0})", inMsg);

            MemoryStream ms = new MemoryStream();

            XmlWriter _Writer = XmlWriter.Create(ms);

            _Writer.WriteStartDocument();
            _Writer.WriteStartElement(RootNodeNameLabel);

            string strPartname;

            for (int i = 0; i < inMsg.PartCount; i++)
            {
                IBaseMessagePart _BaseMessagePart = inMsg.GetPartByIndex(i, out strPartname);

                XmlReader reader = new XmlTextReader(_BaseMessagePart.Data);

                _Writer.WriteNode(reader, true);
            }

            _Writer.WriteEndElement();
            _Writer.WriteEndDocument();

            _Writer.Flush();

            ms.Position = 0;

            return ms;
        }

        #endregion
    }
}
