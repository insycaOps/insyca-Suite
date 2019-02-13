
//
// DocSpecLoader.cs
//
// Author:
//    Tomas Restrepo (tomasr@mvps.org)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;


using Microsoft.BizTalk.PipelineOM;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.XLANGs.BaseTypes;


namespace Winterdom.BizTalk.PipelineTesting
{
   /// <summary>
   /// This class does the bulk of the work in loading
   /// document specifications (DocSpecs).
   /// </summary>
   public class DocSpecLoader
   {

      /// <summary>
      /// Creates a document specification from a CLR
      /// Type representing the root node of a BizTalk
      /// compiled Schema
      /// </summary>
      /// <param name="schemaType">The schema to create</param>
      /// <returns>The document specification object</returns>
      public IDocumentSpec LoadDocSpec(Type schemaType)
      {
         if ( schemaType == null )
            throw new ArgumentNullException("schemaType");
         if ( !schemaType.IsSubclassOf(typeof(SchemaBase)) )
            throw new ArgumentException("Type does not represent a schema", "schemaType");

         string typename = schemaType.FullName;
         string assemblyName = schemaType.Assembly.FullName;
         DocumentSpec docSpec = new DocumentSpec(typename, assemblyName);

         return docSpec;
      }

      /// <summary>
      /// Creates a document specification from a CLR
      /// Type based on its name and containing assembly
      /// </summary>
      /// <param name="typeName">The fully qualified (namespace.class) name of 
      /// the schema</param>
      /// <param name="assemblyName">The partial or full name of the assembly
      /// containing the schema</param>
      /// <returns>The document specification object</returns>
      public IDocumentSpec LoadDocSpec(string typeName, string assemblyName)
      {
         if ( String.IsNullOrEmpty(typeName) )
            throw new ArgumentNullException("typeName");
         if ( String.IsNullOrEmpty(assemblyName) )
            throw new ArgumentNullException("assemblyName");

         return new DocumentSpec(typeName, assemblyName);
      }
   } // class MessageFactory

} // namespace Winterdom.BizTalk.PipelineTesting
