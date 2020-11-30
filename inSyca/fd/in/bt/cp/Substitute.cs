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
using inSyca.foundation.framework.configuration;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;


namespace inSyca.foundation.integration.biztalk.components
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Decoder)]
    [Guid("410689B8-9B77-43DF-9804-A815CF8AC985")]
    public class Substitute : IBaseComponent, IComponent, IPersistPropertyBag, IComponentUI
    {
        static private readonly ResourceManager _resourceManager = new ResourceManager("inSyca.foundation.integration.biztalk.components.Properties.Resources", Assembly.GetExecutingAssembly());

        static string ConfigFilePathLabel = "ConfigFilePath";
        static string XmlElementLowercaseLabel = "XmlElementLowercase";

        #region Properties
        [Description("Path to Configuration File: " + "\r\n" + "There is a sample of this file.")]
        [DisplayName("ConfigFilePath")]
        [DefaultValue("")]
        public string ConfigFilePath { get; set; }

        [Description("Lowercase all XML element nodes")]
        [DisplayName("XmlElementLowercase")]
        [DefaultValue(true)]
        public bool XmlElementLowercase { get; set; }

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
                return _resourceManager.GetString("substitute_description", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("substitute_name", CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("substitute_version", CultureInfo.InvariantCulture);
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
            classID = new System.Guid("410689B8-9B77-43DF-9804-A815CF8AC985");

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

                val = PropertyHelper.ReadPropertyBag(propertyBag, ConfigFilePathLabel);
               
                if (val != null)
                    ConfigFilePath = (string)val;

                val = PropertyHelper.ReadPropertyBag(propertyBag, XmlElementLowercaseLabel);
               
                if (val != null)
                    XmlElementLowercase = (bool)val;
            }

            Log.DebugFormat("Load ConfigFilePath {0}", ConfigFilePath);
            Log.DebugFormat("Load XmlElementLowercase {0}", XmlElementLowercase);
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

                val = ConfigFilePath;
                propertyBag.Write(ConfigFilePathLabel, ref val);

                val = XmlElementLowercase;
                propertyBag.Write(XmlElementLowercaseLabel, ref val);
            }

            Log.DebugFormat("Save ConfigFilePath {0}", ConfigFilePath);
            Log.DebugFormat("Save XmlElementLowercase {0}", XmlElementLowercase);
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

        #region IComponent Members

        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            Log.DebugFormat("Execute(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})", pContext, pInMsg);

            try
            {
                if (pInMsg == null || pInMsg.BodyPart == null || pInMsg.BodyPart.Data == null)
                {
                    Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, "pInMsg Error", null));
                    throw new ArgumentNullException("pInMsg Error");
                }

                var bodyPart = pInMsg.BodyPart;

                var inboundStream = bodyPart.GetOriginalDataStream();

                MemoryStream stream = SubstituteStream(inboundStream);

                Log.DebugFormat("Execute(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})\nstream.Length={2})", pContext, pInMsg, stream.Length);

                stream.Seek(0, SeekOrigin.Begin);
                pInMsg.BodyPart.Data = stream;
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, ex));
            }

            return pInMsg;
        }

        #endregion

        #region Helper

        private MemoryStream SubstituteStream(Stream inputStream)
        {
            string messageBody = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(ConfigFilePath))
                {
                    RegistryKey regKey = null;

                    regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\inSyca\foundation.integration.biztalk.components", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);

                    if (regKey != null)
                        ConfigFilePath = (string)regKey.GetValue("SubstituteConfiguration");
                }

                Parameters configParameters = new Parameters();
                configParameters.Initialize(ConfigFilePath);

                StreamReader reader = new StreamReader(inputStream);
                messageBody = reader.ReadToEnd();

                // simple replacement:
                foreach (Parameter p in configParameters.SimpleParameter)
                {
                    if (p.Search != null || p.ReplaceTo != null) // check
                        if (p.Search.Length > 0) // check
                        {
                            Log.DebugFormat("SubstituteStream(Stream inputStream {0})\nSimple - p.Search={1}\np.ReplaceTo={2}", inputStream, p.Search, p.ReplaceTo );
                            messageBody = messageBody.Replace(p.Search, p.ReplaceTo);
                        }
                }

                // RegEx replacement:
                foreach (Parameter p in configParameters.RegExParameter)
                {
                    Regex rx = null;
                    if (p.Search != null || p.ReplaceTo != null) // check
                        if (p.Search.Length > 0) // check
                        {
                            Log.DebugFormat("SubstituteStream(Stream inputStream {0})\nRegex - p.Search={1}\np.ReplaceTo={2}", inputStream, p.Search, p.ReplaceTo);
                            rx = new Regex(p.Search);
                            messageBody = rx.Replace(messageBody, p.ReplaceTo);
                        }
                }

                Log.DebugFormat("SubstituteStream(Stream inputStream {0})\nmessageBody= {1}", inputStream, messageBody);
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, ex));
            }

            MemoryStream ms;

            if (XmlElementLowercase)
            {
                ms = new MemoryStream();
                try
                {
                    string sXslt = string.Concat(new string[]{
                    "<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>",
                    "<xsl:output method='xml' indent='yes'/>",
                    "<xsl:variable name='lowercase' select='abcdefghijklmnopqrstuvwxyz'/>",
                    "<xsl:variable name='uppercase' select='ABCDEFGHIJKLMNOPQRSTUVWXYZ' />",
                    "<xsl:template match='*'>",
                    "<xsl:element name='{ translate(local-name(), $uppercase, $lowercase)}' namespace='{namespace-uri()}'>",
                    "<xsl:apply-templates/>",
                    "</xsl:element>",
                    "</xsl:template>",
                    "</xsl:stylesheet>"});

                    Log.DebugFormat("SubstituteStream(Stream inputStream {0})\nsXslt= {1}", inputStream, sXslt );

                    XslCompiledTransform objXslTrans = new XslCompiledTransform();
                    objXslTrans.Load(new XmlTextReader(new StringReader(sXslt)));
                    XmlWriter xmlWriter = XmlWriter.Create(ms);
                    objXslTrans.Transform(new XmlTextReader(new StringReader(messageBody)), xmlWriter);

                    xmlWriter.Flush();
                }
                catch (Exception ex)
                {
                    Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, ex));
                }
            }
            else
            {
                ms = new MemoryStream(Encoding.UTF8.GetBytes(messageBody ?? ""));
            }

            return ms;
        }

        #endregion
    }
}
