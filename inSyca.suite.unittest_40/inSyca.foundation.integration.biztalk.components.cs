﻿using inSyca.foundation.framework.conversion;
using inSyca.foundation.integration.biztalk.components;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Winterdom.BizTalk.PipelineTesting;

namespace inSyca.foundation.unittest_40
{
	[TestClass]
    public class testComponents
    {

        [TestMethod]
        public void PipelineComponent_RemoveNilAndEmpty()
        {
            // Create the input message to pass through the pipeline
            Stream stream = StreamConverter.StringToStream(XElement.Load(@"..\..\Testfiles\simple_002.xml").ToString(), Encoding.UTF8);
            IBaseMessage inputMessage = MessageHelper.CreateFromStream(stream);

            RemoveNil removeNil = new RemoveNil();

            // Execute the pipeline, and check the output
            ReceivePipelineWrapper pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            pipeline.AddComponent(removeNil, PipelineStage.Disassemble);
            MessageCollection outputMessages = pipeline.Execute(inputMessage);

            foreach (var message in outputMessages)
            {
                Console.WriteLine("************************** start message **************************\r\n");
                Console.WriteLine(StreamConverter.StreamToString(message.BodyPart.Data, Encoding.UTF8));
                Console.WriteLine("\r\n************************** end message **************************");
            }
        }

        [TestMethod]
        public void PipelineComponent_RemoveNamespace()
        {
            // Create the input message to pass through the pipeline
            Stream stream = StreamConverter.StringToStream(XElement.Load(@"..\..\Testfiles\simple_002.xml").ToString(), Encoding.UTF8);
            IBaseMessage inputMessage = MessageHelper.CreateFromStream(stream);

            RemoveNamespace component = new RemoveNamespace();

            // Execute the pipeline, and check the output
            ReceivePipelineWrapper pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            pipeline.AddComponent(component, PipelineStage.Disassemble);
            MessageCollection outputMessages = pipeline.Execute(inputMessage);

            foreach (var message in outputMessages)
            {
                Console.WriteLine("************************** start message **************************\r\n");
                Console.WriteLine(StreamConverter.StreamToString(message.BodyPart.Data, Encoding.UTF8));
                Console.WriteLine("\r\n************************** end message **************************");
            }
        }

        [TestMethod]
        public void PipelineComponent_XmlSplitter()
        {
            // Create the input message to pass through the pipeline
            Stream stream = StreamConverter.StringToStream(XElement.Load(@"..\..\Testfiles\simple_002.xml").ToString(), Encoding.UTF8);
            IBaseMessage inputMessage = MessageHelper.CreateFromStream(stream);

            XmlSplitter xmlSplitter = new XmlSplitter();
            xmlSplitter.ChildNodeName = "food";
            xmlSplitter.GroupByNodeName = "name";

            // Execute the pipeline, and check the output
            ReceivePipelineWrapper pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            pipeline.AddComponent(xmlSplitter, PipelineStage.Disassemble);
            MessageCollection outputMessages = pipeline.Execute(inputMessage);

            foreach (var message in outputMessages)
            {
                Console.WriteLine("************************** start message **************************\r\n");
                Console.WriteLine(StreamConverter.StreamToString(message.BodyPart.Data, Encoding.UTF8));
                Console.WriteLine("\r\n************************** end message **************************");
            }
        }

        [TestMethod]
        public void PipelineComponent_ActiveXReader()
        {
            // Create the input message to pass through the pipeline
            Stream stream = StreamConverter.StringToStream(XElement.Load(@"..\..\Testfiles\simple_002.xml").ToString(), Encoding.UTF8);
            IBaseMessage inputMessage = MessageHelper.CreateFromStream(stream);

            ActiveXMessageReader activeXMessageReader = new ActiveXMessageReader();
            activeXMessageReader.IncomingEncoding = "utf-16";

            // Execute the pipeline, and check the output
            ReceivePipelineWrapper pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            pipeline.AddComponent(activeXMessageReader, PipelineStage.Decode);
            MessageCollection outputMessages = pipeline.Execute(inputMessage);

            foreach (var message in outputMessages)
            {
                Console.WriteLine("************************** start message **************************\r\n");
                Console.WriteLine(StreamConverter.StreamToString(message.BodyPart.Data, Encoding.UTF8));
                Console.WriteLine("\r\n************************** end message **************************");
            }
        }
    }
}
