//---------------------------------------------------------------------
// File: XslTransformer.cs
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
using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml;
using System.Xml.Xsl;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;

namespace inSyca.foundation.integration.biztalk.components
{
    /// <summary>
    /// Implements a pipeline component that applies Xsl Transformations to XML messages
    /// </summary>
    /// <remarks>
    /// XslTransformer class implements pipeline components that can be used in send pipelines
    /// to convert XML messages to HTML format for sending using SMTP transport. Component can
    /// be placed only in Encoding stage of send pipeline
    /// </remarks>
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Serializer)]
	[System.Runtime.InteropServices.Guid("FA7F9C55-6E8E-4855-8DAC-FA1BC8A499E2")]
	public class XslTransformer : IBaseComponent, IComponent, IPersistPropertyBag, IComponentUI
	{
        static private readonly ResourceManager _resourceManager = new ResourceManager("inSyca.foundation.integration.biztalk.components.Properties.Resources", Assembly.GetExecutingAssembly());

        static string XsltFilePathLabel = "XsltFilePath";

        #region Properties
        [Description("XsltFilePath")]
        [DisplayName("XsltFilePath")]
        [DefaultValue("")]
        public string XsltFilePath { get; set; }
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
                return _resourceManager.GetString("xslTransformer_description", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("xslTransformer_name", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("xslTransformer_version", CultureInfo.InvariantCulture);
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
            classID = new System.Guid("FA7F9C55-6E8E-4855-8DAC-FA1BC8A499E2");

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

                val = PropertyHelper.ReadPropertyBag(propertyBag, XsltFilePathLabel);

                if (val != null)
                    XsltFilePath = (string)val;
            }

            Log.DebugFormat("Load XsltFilePath {0}", XsltFilePath);
        }

        /// <summary>
        /// Saves the current component configuration into the property bag
        /// </summary>
        /// <param name="propertyBag">Configuration property bag</param>
        /// <param name="clearDirty">not used</param>
        /// <param name="saveAllProperties">not used</param>
        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;

                val = XsltFilePath;
                propertyBag.Write(XsltFilePathLabel, ref val);
            }

            Log.DebugFormat("Save XsltFilePath {0}", XsltFilePath);
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
            IEnumerator enumerator = null;
            ArrayList strList = new ArrayList();

            try
            {
                GetValidXsltPath(XsltFilePath);
            }
            catch (Exception e)
            {
                strList.Add(e.Message);
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
			inMsg.BodyPart.ContentType = "text/html";
			return inMsg;
		}
        #endregion

        #region Helper
        /// <summary>
        /// Transforms XML message in input stream to HTML message
        /// </summary>
        /// <param name="stream">Stream with input XML message</param>
        /// <returns>Stream with output HTML message</returns>
        private Stream TransformMessage(Stream stream)
		{
            Log.DebugFormat("TransformMessage(Stream stream {0})", stream);

            MemoryStream ms = new MemoryStream();
			string validXsltPath = null;
			
			try 
			{
				// Get the full path to the Xslt file
                validXsltPath = GetValidXsltPath(XsltFilePath);
				
				// Load transform
				XslCompiledTransform transform = new XslCompiledTransform();
				transform.Load(validXsltPath);
				
				//Load Xml stream in XmlDocument. OLD VERSION
				//XmlDocument doc = new XmlDocument();
				//doc.Load(stm);

				XmlReader reader = new XmlTextReader(stream);
				
				//Create memory stream to hold transformed data.
				ms = new MemoryStream();
				
				//Preform transform
				transform.Transform(reader, null, ms);
				ms.Seek(0, SeekOrigin.Begin);
			}
			catch(Exception ex) 
			{
				Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { stream }, ex));
				throw ex;
			}

			return ms;
		}

		/// <summary>
		/// Get a valid full path to a Xslt file
		/// </summary>
		/// <param name="path">Path provided by user in Pipeline Designer</param>
		/// <returns>The full path</returns>
		/// <remarks>
		/// If user provides absolute path then it is used as long as the file can be opened there
		/// If user provides just a name of file or relative path then we try to open a file in 
		/// [Install foder]\Pipeline Components
		/// </remarks>
		private string GetValidXsltPath(string path)
		{
            Log.DebugFormat("GetValidXsltPath(string path {0})", path);

            string validPath = path;

			if (!System.IO.File.Exists(path))
			{
						RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\integration Server\3.0");
						string InstallPath = string.Empty;
				
						if (null != rk)
					InstallPath = (String)rk.GetValue("InstallPath");
						
						validPath = InstallPath + @"Pipeline Components\" + path;
				if (!System.IO.File.Exists(validPath))
				{
					throw new ArgumentException("The XSL transformation file " + path + " can not be found");
				}
			}	

			return validPath;
		}

		#endregion	
	}
}
