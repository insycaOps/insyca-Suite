﻿namespace inSyca.foundation.integration.biztalk.components
{
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.BizTalk.Message.Interop;
    using Microsoft.BizTalk.Component.Interop;
    using Microsoft.BizTalk.Component;
    using System.Runtime.InteropServices;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using diagnostics;
    using System.ComponentModel;

    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
    [Guid("16D92957-2034-4853-840B-BD424C596247")]
    public class FFAttachmentsExtractor : FFDasmComp, IBaseComponent, IComponentUI, IDisassemblerComponent, IPersistPropertyBag
    {
        private string regExReplacement = "\".*?\"";

        [Browsable(true)]
        public string RegExReplacement
        {
            get { return regExReplacement; }
            set { regExReplacement = value; }
        }

        #region IBaseComponent Members
        /// <summary>
        /// Gets Description of the component
        /// </summary>   
        [Browsable(false)]
        public new string Description
        {
            get
            {
                return "FlatFile Email Attachments Extractor";
            }
        }

        /// <summary>
        /// Gets Name of the component
        /// </summary>   
        [Browsable(false)]
        public new string Name
        {
            get
            {
                return "FFEmailAttachmentsExtractor";
            }
        }

        /// <summary>
        /// Gets Version of the component
        /// </summary>
        [Browsable(false)]
        public new string Version
        {
            get
            {
                return "1.0";
            }
        }
        #endregion

        #region IPersistPropertyBag

        /// <summary>
        /// Gets class ID of component for usage from unmanaged code.
        /// </summary>
        /// <param name="classid">Class ID of the component.</param>
        public new void GetClassID(out Guid classid)
        {
            Log.DebugFormat("GetClassID(out Guid classid)");

            classid = new System.Guid("16D92957-2034-4853-840B-BD424C596247");
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public new void InitNew()
        {
        }

        public new void Load(IPropertyBag propertyBag, int errorLog)
        {
            Log.DebugFormat("Load(IPropertyBag propertyBag {0} , int errorLog {1})", propertyBag, errorLog);

            base.Load(propertyBag, errorLog);

            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;
                val = PropertyHelper.ReadPropertyBag(propertyBag, "RegExReplacement");
                if (val != null)
                {
                    regExReplacement = (string)val;
                }
            }

            Log.DebugFormat("Load RegExReplacement {0}", regExReplacement);
        }

        public new void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            Log.DebugFormat("Load(IPropertyBag propertyBag {0}, bool clearDirty {1}, bool saveAllProperties {2})", propertyBag, clearDirty, saveAllProperties);

            base.Save(propertyBag, clearDirty, saveAllProperties);

            using (DisposableObjectWrapper wrapper = new DisposableObjectWrapper(propertyBag))
            {
                object val = null;
                val = regExReplacement;
                propertyBag.Write("RegExReplacement", ref val);
            }

            Log.DebugFormat("Save RegExReplacement {0}", regExReplacement);
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

        #endregion

        /// <summary>
        /// this variable will contain any message generated by the Disassemble method
        /// </summary>
     //   private System.Collections.Queue _msgs = new System.Collections.Queue();

        #region IDisassemblerComponent members

        /// <summary>
        /// Returns messages resulting from the disassemble method execution
        /// </summary>
        /// <param name="pipelineContext">the pipeline context</param>
        /// <returns></returns>
        //public new IBaseMessage GetNext(IPipelineContext pc)
        //{
        //    // get the next message from the Queue and return it
        //    IBaseMessage msg = null;
        //    if ((_msgs.Count > 0))
        //    {
        //        msg = ((IBaseMessage)(_msgs.Dequeue()));
        //    }
        //    return msg;
        //}

        /// <summary>
        /// called by the messaging engine when a new message arrives
        /// </summary>
        /// <param name="pipelineContext">the pipeline context</param>
        /// <param name="inMsg">the actual message</param>
        public new void Disassemble(Microsoft.BizTalk.Component.Interop.IPipelineContext pipelineContext, Microsoft.BizTalk.Message.Interop.IBaseMessage inMsg)
        {
            Log.DebugFormat("Execute(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})", pipelineContext, inMsg);

            var partName = string.Empty;
            // we start from index 1 because index zero contains the body of the message
            // which we are not interested
            for (int i = 1; i < inMsg.PartCount; i++)
            {
                Log.DebugFormat("PartCounter {0}", i);

                IBaseMessagePart currentPart = inMsg.GetPartByIndex(i, out partName);

                Stream currentPartStream = currentPart.GetOriginalDataStream();

                IBaseMessage outMsg;
                outMsg = pipelineContext.GetMessageFactory().CreateMessage();

                for (int j = 0; j < inMsg.Context.CountProperties; j++)
                {

                    string currentName;
                    string currentNamespace;
                    object obj = inMsg.Context.ReadAt(j, out currentName, out currentNamespace);
                    outMsg.Context.Write(currentName, currentNamespace, obj);

                    if (inMsg.Context.IsPromoted(currentName, currentNamespace))
                    {
                        outMsg.Context.Promote(currentName, currentNamespace, obj);
                    }
                }

                if (IsValidImage(currentPartStream))
                {
                    continue;
                }

                Stream ms;

                Log.DebugFormat("regExReplacement {0}", regExReplacement);

                if (string.IsNullOrEmpty(regExReplacement))
                {
                    Log.DebugFormat("No RegExReplacement");

                    ms = new MemoryStream();
                    currentPartStream.CopyTo(ms);
                }
                else
                {
                    StreamReader sr = new StreamReader(currentPartStream);

                    string messageString = sr.ReadToEnd();

                    Log.DebugFormat("messageString before processing{0}", messageString);

                    messageString = messageString.TrimEnd('\r', '\n');
                    messageString = messageString + Environment.NewLine;

                    //                    Regex rgx = new Regex("\".*?\"");
                    Regex rgx = new Regex(RegExReplacement);

                    messageString = rgx.Replace(messageString, "");

                    Log.DebugFormat("messageString after processing{0}", messageString);

                    byte[] byteArray = Encoding.UTF8.GetBytes(messageString);

                    ms = new MemoryStream(byteArray);
                }

                ms.Seek(0, SeekOrigin.Begin);
                outMsg.AddPart("Body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
                outMsg.BodyPart.Data = ms;

                //Promote attachment file name
                outMsg.Context.Write("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/file-properties", currentPart.PartProperties.Read("FileName", "http://schemas.microsoft.com/BizTalk/2003/mime-properties"));

                base.Disassemble(pipelineContext, outMsg);

                Log.DebugFormat("Flatfile Dissasembler(IPipelineContext pContext {0}, IBaseMessage pInMsg {1})", pipelineContext, outMsg);

                //_msgs.Enqueue(outMsg);
            }
        }
        private bool IsValidImage(Stream ms)
        {
            try
            {
                Image.FromStream(ms);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
