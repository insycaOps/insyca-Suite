
//
// ReceivePipelineWrapper.cs
//
// Author:
//    Tomas Restrepo (tomasr@mvps.org)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.BizTalk.PipelineOM;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
using IPipeline = Microsoft.Test.BizTalk.PipelineObjects.IPipeline;
using PStage = Microsoft.Test.BizTalk.PipelineObjects.Stage;


namespace Winterdom.BizTalk.PipelineTesting
{
   /// <summary>
   /// Wrapper around a receive pipeline you can execute
   /// </summary>
   public class ReceivePipelineWrapper : BasePipelineWrapper
   {

      internal ReceivePipelineWrapper(IPipeline pipeline)
         : base(pipeline, true)
      {
         FindStage(PipelineStage.Decode);
         FindStage(PipelineStage.Disassemble);
         FindStage(PipelineStage.Validate);
         FindStage(PipelineStage.ResolveParty);
      }

      /// <summary>
      /// Executes the receive pipeline
      /// </summary>
      /// <param name="inputMessage">Input message to feed to the pipeline</param>
      /// <returns>A collection of messages that were generated by the pipeline</returns>
      public MessageCollection Execute(IBaseMessage inputMessage)
      {
         if ( inputMessage == null )
            throw new ArgumentNullException("inputMessage");

         Pipeline.InputMessages.Add(inputMessage);
         MessageCollection output = new MessageCollection();
         Pipeline.Execute(Context);

         IBaseMessage om = null;
         while ( (om = Pipeline.GetNextOutputMessage(Context)) != null )
         {
            output.Add(om);
            // we have to consume the entire stream for the body part.
            // Otherwise, the disassembler might enter an infinite loop.
            // We currently copy the output into a new memory stream
            if ( om.BodyPart != null && om.BodyPart.Data != null ) {
               om.BodyPart.Data = CopyStream(om.BodyPart.Data);
            }
        }

         return output;
      }

      private Stream CopyStream(Stream source)
      {
         MemoryStream stream = new MemoryStream();

         byte[] buffer = new byte[64 * 1024];
         int bytesRead = 0;
         while ( (bytesRead = source.Read(buffer, 0, buffer.Length)) > 0 )
         {
            stream.Write(buffer, 0, bytesRead);
         }
         stream.Position = 0;
         return stream;
      }

   } // class ReceivePipelineWrapper

} // namespace Winterdom.BizTalk.PipelineTesting
