﻿using inSyca.messagebroker.root.ns.interfaces;
using inSyca.foundation.communication.clients;
using inSyca.foundation.communication.wcf;
using inSyca.foundation.framework;
using System;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace inSyca.messagebroker.ns
{
    public partial class MessageBrokerService : ICompany
    {
        StringBuilder dbcommandbuilder = new StringBuilder();
        //System.Data.OleDb.OleDbCommand commandmessagebroker;
        //System.Data.OleDb.OleDbDataReader readermessagebroker;
        //int rowCount = 0;

        string strSQLDatabaseSqlConnectionString = string.Empty;

        public string SQLDatabaseSqlConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(strSQLDatabaseSqlConnectionString))
                    strSQLDatabaseSqlConnectionString = Configuration.GetConnectionStringsValue("SQLDatabase_SQL");

                return strSQLDatabaseSqlConnectionString;
            }
        }

        private bool Init_SQLDatabase()
        {
            string strSQLDatabaseOleDbConnectionString;

            try
            {
                strSQLDatabaseOleDbConnectionString = Configuration.GetConnectionStringsValue("SQLDatabase_OLEDB");
            }
            catch (Exception ex)
            {
                Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, ex));
                return false;
            }

            if (!OpenOleDbSQLConnection(strSQLDatabaseOleDbConnectionString))
                return false;

            return true;
        }

        private bool Dispose_SQLDatabase()
        {
            if (!CloseOleDbSQLConnection())
                return false;

            return true;
        }

        private bool getSQLTable()
        {
            /*
            dbcommandbuilder.Length = 0;

            dbcommandbuilder.Append("SELECT TOP 100 * ");
            dbcommandbuilder.Append("FROM TABLE_NAME");
            dbcommandbuilder.Append(" WHERE TABLE_FIELDNAME IS NULL");

            commandmessagebroker = new System.Data.OleDb.OleDbCommand(dbcommandbuilder.ToString(), OleDbSqlConnection_MessageBroker);

            readermessagebroker = commandmessagebroker.ExecuteReader();

            if (readermessagebroker.HasRows)
            {
                List<orders.orders_requestOrder> listRequestOrders = new List<orders.orders_requestOrder>();

                try
                {
                    while (readermessagebroker.Read())
                    {
                        orders.orders_requestOrder orders_requestOrder = new orders.orders_requestOrder();

                        orders_requestOrder.datafield = string.Format("{0}", readermessagebroker["TABLE_FIELDNAME"]);
                        orders_requestOrder.datafield = string.Format("{0}", readermessagebroker["TABLE_FIELDNAME"]);
                        orders_requestOrder.datafield = string.Format("{0}", readermessagebroker["TABLE_FIELDNAME"]);
                        orders_requestOrder.datafield = string.Format("{0}", readermessagebroker["TABLE_FIELDNAME"]);
                        if (!readermessagebroker.IsDBNull(readermessagebroker.GetOrdinal("TABLE_FIELDNAME")))
                            orders_requestOrder.datafield = string.Format("{0}", readermessagebroker["TABLE_FIELDNAME"]);

                        listRequestOrders.Add(orders_requestOrder);

                        dbcommandbuilder.Length = 0;

                        dbcommandbuilder.Append("UPDATE TABLE_NAME ");
                        dbcommandbuilder.Append("SET TABLE_FIELDNAME = N'BIZTALK' ");
                        dbcommandbuilder.Append(string.Format("WHERE TABLE_FIELDNAME = {0} ", readermessagebroker["TABLE_FIELDNAME"]));

                        commandmessagebroker = new System.Data.OleDb.OleDbCommand(dbcommandbuilder.ToString(), OleDbSqlConnection_MessageBroker);
                        commandmessagebroker.ExecuteNonQuery();
                    }
                    using (orders.svc_ordersClient client = new orders.svc_ordersClient())
                    {
                        client.Open();

                        client.op_orders(listRequestOrders.ToArray());
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), new object[] { listRequestOrders }, ex));
                }
            }
            */

            return true;
        }

        public bool setSQLTable(BizTalkMessageWrapper inDocument, out BizTalkMessageWrapper outDocument)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Log.Info("--- SERVICE CALL: setSQLTable(BizTalkMessageWrapper inDocument, out BizTalkMessageWrapper outDocument) ---");

            Console.WriteLine(inDocument.BizTalkMessage.ToString());

            Console.ForegroundColor = ConsoleColor.White;

            InitServiceResponse(out outDocument);

            serviceResponseRow.status = 0;
            serviceResponseRow.message = "SUCCESS";

            /*

            try
            {
                using (sqlBulkCopy = new SqlBulkCopy(SQLDatabaseSqlConnectionString, SqlBulkCopyOptions.TableLock))
                {
                    sqlBulkCopy.DestinationTableName = "TABLE_NAME";

                    sqlBulkCopy.ColumnMappings.Add("SCHEMA_FIELDNAME", "TABLE_FIELDNAME");
                    sqlBulkCopy.ColumnMappings.Add("SCHEMA_FIELDNAME", "TABLE_FIELDNAME");
                    sqlBulkCopy.ColumnMappings.Add("SCHEMA_FIELDNAME", "TABLE_FIELDNAME");
                    sqlBulkCopy.ColumnMappings.Add("SCHEMA_FIELDNAME", "TABLE_FIELDNAME");
                    sqlBulkCopy.ColumnMappings.Add("SCHEMA_FIELDNAME", "TABLE_FIELDNAME");

                    if (!ExecuteSqlBulkCopy(inDocument.BizTalkDataSet.Tables["TABLE_NAME"]))
                    {
                        serviceResponseRow["status"] = 1;
                        serviceResponseRow["message"] = "ERROR";
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponseRow["status"] = 1;
                serviceResponseRow["message"] = ex.Message;
            }
*/
            Thread.Sleep(500);

            AssembleServiceResponse(outDocument);

            return true;
        }

        public void sendToBizTalkMessageBox()
        {
            using (BizTalkClient btClient = new BizTalkClient())
            {
                try
                {
                    XmlReader xmlrdr = XElement.Parse("<ns0:message xmlns:ns0='http://inSyca.foundation.messagebroker/testmessage'><id>69</id><trackingid>223FCA9C-EB79-46F5-86F0-5AA6CA66553B</trackingid></ns0:message>").CreateReader();
                    Message message = Message.CreateMessage(MessageVersion.Default, "*", xmlrdr);

                    btClient.SendToMsgBox(message);
                }
                catch (Exception ex)
                {
                    Log.Error(new LogEntry(System.Reflection.MethodBase.GetCurrentMethod(), null, ex));
                }
            }
        }
    }
}
