//---------------------------------------------------------------------
// File: StripMessageBrokerBody.cs
// 
// Summary: Pipeline component that applies Xsl transformation to convert XML messages to HTML documents.
//
// Sample: Xsl Transformation component SDK 
//
//---------------------------------------------------------------------
// This file is part of the Microsoft integration Server 2009 SDK
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// This source code is intended only as a supplement to Microsoft integration
// Server 2009 release and/or on-line documentation. See these other
// materials for detailed information regarding Microsoft code samples.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
// PURPOSE.
//---------------------------------------------------------------------

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
    /// <summary>
    /// Implements a pipeline component that applies Xsl Transformations to XML messages
    /// </summary>
    /// <remarks>
    /// StripMessageBrokerBody class implements pipeline components that can be used in send pipelines
    /// to convert XML messages to HTML format for sending using SMTP transport. Component can
    /// be placed only in Encoding stage of send pipeline
    /// </remarks>
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Decoder)]
    [System.Runtime.InteropServices.Guid("0B451084-3BCF-41A0-92C3-51E1A9A8621E")]
    public class StripMessageBrokerBody : IBaseComponent, IComponent, IPersistPropertyBag, IComponentUI
    {
        static private readonly ResourceManager _resourceManager = new ResourceManager("inSyca.foundation.integration.biztalk.components.Properties.Resources", Assembly.GetExecutingAssembly());

        static string OuterXmlNodeNameLabel = "OuterXmlNodeName";

        #region Properties
        /// <summary>
        /// Location of Xsl transform file.
        /// </summary>
        [Description("OuterXmlNodeName")]
        [DisplayName("OuterXmlNodeName")]
        [DefaultValue("part")]
        public string OuterXmlNodeName { get; set; }
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
                return _resourceManager.GetString("stripMessageBrokerBody_description", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("stripMessageBrokerBody_name", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("stripMessageBrokerBody_version", CultureInfo.InvariantCulture);
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
            classID = new System.Guid("0B451084-3BCF-41A0-92C3-51E1A9A8621E");

            Log.DebugFormat("GetClassID(out Guid {0})", classID);
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public void InitNew()
        {
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

                val = PropertyHelper.ReadPropertyBag(propertyBag, OuterXmlNodeNameLabel);

                if (val != null)
                    OuterXmlNodeName = (string)val;
            }

            Log.DebugFormat("Load OuterXmlNodeName {0}", OuterXmlNodeName);
        }


        /// <summary>
        /// Saves the current component configuration into the property bag
        /// </summary>
        /// <param name="propertyBag">Configuration property bag</param>
        /// <param name="clearDirty">not used</param>
        /// <param name="saveAllProperties">not used</param>
        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            Log.DebugFormat("Load(IPropertyBag propertyBag {0}, bool clearDirty {1}, bool saveAllProperties {2})", propertyBag, clearDirty, saveAllProperties);

            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;

                val = OuterXmlNodeName;
                propertyBag.Write(OuterXmlNodeNameLabel, ref val);
            }

            Log.DebugFormat("Save OuterXmlNodeName {0}", OuterXmlNodeName);
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

        #region IComponent

        /// <summary>
        /// Implements IComponent.Execute method.
        /// </summary>
        /// <param name="pipelineContext">Pipeline context</param>
        /// <param name="inMsg">Input message.</param>
        /// <returns>Converted to HTML input message.</returns>
        /// <remarks>
        /// IComponent.Execute method is used to convert XML messages
        /// to HTML messages using provided Xslt file.
        /// It also sets the content type of the message part to be "text/html"
        /// which is necessary for client mail applications to correctly render
        /// the message
        /// </remarks>
        public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage inMsg)
        {
            Log.DebugFormat("Execute(IPipelineContext pipelineContext {0}, IBaseMessage inMsg {1})", pipelineContext, inMsg);

            inMsg.BodyPart.Data = TransformMessage(inMsg.BodyPart.Data);
            return inMsg;
        }
        #endregion

        #region Helper function
        /// <summary>
        /// Transforms XML message in input stream to HTML message
        /// </summary>
        /// <param name="stream">Stream with input XML message</param>
        /// <returns>Stream with output HTML message</returns>
        private Stream TransformMessage(Stream stream)
        {
            Log.DebugFormat("TransformMessage(Stream stream {0})", stream);

            MemoryStream ms = new MemoryStream();

            try
            {
                using (XmlWriter _Writer = XmlWriter.Create(ms))
                { 
                    _Writer.WriteStartDocument();

                    using (XmlReader outerReader = new XmlTextReader(stream))
                    {
                        outerReader.ReadToFollowing(OuterXmlNodeName);
                        outerReader.Read();

                        using (XmlReader innerReader = outerReader.ReadSubtree())
                            while (!innerReader.EOF)
                                _Writer.WriteNode(innerReader, false);
                    }

                    _Writer.WriteEndDocument();

                    _Writer.Flush();
                }
                ms.Position = 0;
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(MethodBase.GetCurrentMethod(), new object[] { stream }, ex));
                throw ex;
            }

            return ms;
        }

        #endregion
    }
}
