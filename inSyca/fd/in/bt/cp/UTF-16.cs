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
using System.Runtime.InteropServices;
using System.Text;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;

namespace inSyca.foundation.integration.biztalk.components
{
    /// <summary>
    /// Implements custom pipeline component to append and/or prepend data to a stream.
    /// </summary>
    /// <remarks>
    /// FixMag class implements pipeline component that can be used in receive and
    /// send integration pipelines. The pipeline component gets a data stream, appends
    /// and/or prepends user specified data to it and outputs modified stream.
    ///</remarks>
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    [ComponentCategory(CategoryTypes.CATID_Validate)]
    [Guid("48BEC85A-20EE-40ad-BFD0-319B59A0DDBC")]
    public class UTF_16 : BaseCustomTypeDescriptor, IBaseComponent, IComponent, IPersistPropertyBag, IComponentUI
    {
        /// <summary>
        /// Constructor initializes base class to allow custom names and description for component properies
        /// </summary>
        public UTF_16() :
            base(_resourceManager)
        {
        }

        static private readonly ResourceManager _resourceManager = new ResourceManager("inSyca.foundation.integration.biztalk.components.Properties.Resources", Assembly.GetExecutingAssembly());

        static string PrependDataLabel = "PrependData";
        static string AppendDataLabel = "AppendData";

        #region Properties
        /// <summary>
        /// Data to prepend at the beginning of a stream.
        /// </summary>	
        [Description("PrependData")]
        [DisplayName("PrependData")]
        [DefaultValue("")]
        public string PrependData { get; set; }

        /// <summary>
        /// Data to append at the end of stream.
        /// </summary>
        [Description("AppendData")]
        [DisplayName("AppendData")]
        [DefaultValue("")]
        public string AppendData { get; set; }
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
                return _resourceManager.GetString("UTF_16_description", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("UTF_16_name", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("UTF_16_version", CultureInfo.InvariantCulture);
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
            classID = new System.Guid("16D92957-2034-4853-840B-BD424C596247");

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

                val = PropertyHelper.ReadPropertyBag(propertyBag, AppendDataLabel);

                if (val != null)
                    AppendData = (string)val;

                val = PropertyHelper.ReadPropertyBag(propertyBag, PrependDataLabel);

                if (val != null)
                    PrependData = (string)val;
            }

            Log.DebugFormat("Load AppendData {0}", AppendData);
            Log.DebugFormat("Load PrependData {0}", PrependData);
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

                val = AppendData;
                propertyBag.Write(AppendDataLabel, ref val);

                val = PrependData;
                propertyBag.Write(PrependDataLabel, ref val);
            }

            Log.DebugFormat("Save AppendData {0}", AppendData);
            Log.DebugFormat("Save PrependData {0}", PrependData);
        }
        #endregion

        #region IComponentUI

        /// <summary>
        /// Component icon to use in integration Editor.
        /// </summary>
        public IntPtr Icon
        {
            get
            {
                return Properties.Resources.cog.Handle;
            }
        }

        /// <summary>
        /// The Validate method is called by the integration Editor during the build 
        /// of a integration project.
        /// </summary>
        /// <param name="obj">Project system.</param>
        /// <returns>
        /// A list of error and/or warning messages encounter during validation
        /// of this component.
        /// </returns>
        public IEnumerator Validate(object obj)
        {
            // example implementation:
            // ArrayList errorList = new ArrayList();
            // errorList.Add("This is a compiler error");
            // return errorList.GetEnumerator();

            IEnumerator enumerator = null;
            ArrayList strList = new ArrayList();

            // Validate prepend data property
            if ((PrependData != null) &&
            (PrependData.Length >= 64))
            {
                strList.Add(_resourceManager.GetString("ErrorPrependDataTooLong"));
            }

            // validate append data property
            if ((AppendData != null) &&
            (AppendData.Length >= 64))
            {
                strList.Add(_resourceManager.GetString("ErrorAppendDataTooLong"));
            }

            if (strList.Count > 0)
            {
                enumerator = strList.GetEnumerator();
            }

            return enumerator;
        }

        #endregion

        #region IComponent

        /// <summary>
        /// Implements IComponent.Execute method.
        /// </summary>
        /// <param name="pipelineContext">Pipeline context</param>
        /// <param name="inMsg">Input message.</param>
        /// <returns>Processed input message with appended or prepended data.</returns>
        /// <remarks>
        /// IComponent.Execute method is used to initiate
        /// the processing of the message in pipeline component.
        /// </remarks>
        public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage inMsg)
        {
            Log.DebugFormat("Execute(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})", pipelineContext, inMsg);

            IBaseMessagePart bodyPart = inMsg.BodyPart;
            if (bodyPart != null)
            {
                byte[] prependByteData = new byte[] { 255, 254 }; // UTF16 Byte Order Mark
                byte[] appendByteData = ConvertToBytes(AppendData);
                Stream originalStrm = bodyPart.GetOriginalDataStream();
                Stream strm = null;

                if (originalStrm != null)
                {
                    strm = new FixMsgStream(originalStrm, prependByteData, appendByteData, _resourceManager);
                    bodyPart.Data = strm;
                    pipelineContext.ResourceTracker.AddResource(strm);
                }
            }

            return inMsg;
        }
        #endregion

        #region Helper
        /// <summary>
        /// Converts a string to its byte representation.
        /// </summary>
        /// <param name="str">String to be converted to byte representation.</param>
        /// <returns>Array of bytes that represents the string.</returns>
        private byte[] ConvertToBytes(string str)
        {
            byte[] data = null;

            if (str != null)
                data = UTF8Encoding.UTF8.GetBytes(str);

            return data;
        }
        #endregion
    }
}