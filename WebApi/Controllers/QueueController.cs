using IBM.WMQ;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Controllers
{
    /// <summary>
    /// https://stackoverflow.com/questions/47223553/sending-a-message-using-ibm-mq-getting-2059-error-or-hang-on-construction-of-qu  -Some references
    /// </summary>

    [RoutePrefix("api/queue")]
    public class QueueController : ApiController
    {
        // private string connectionType = MQC.TRANSPORT_MQSERIES_CLIENT;
         private string connectionType = MQC.TRANSPORT_MQSERIES_MANAGED;

        // Define the name of your host connection (applies to client connections only)
        const string hostName = "127.0.0.1";

        // Define the name of the queue manager to use (applies to all connections)
        const string qManager = "QM1";

        // Define the name of the channel to use (applies to client connections only)
        const string channel = "DEV.APP.SVRCONN";

        [HttpPost]
        [Route("queueMessage")]
        public async Task<IHttpActionResult> QueMessage([FromBody] Person request)
        {
            QueueMessage();
            await Task.CompletedTask;
            return Ok();
        }

        private void QueueMessage()
        {
            try
            {
                string userName = Environment.UserName;
                var person = new Person
                {
                    FirstName  ="Rennish",
                    LastName  ="Joseph",
                    CreditCardNumber = "2345678910",
                    Email  ="test@email.com",
                    Id = 1
                };
                var personXml = person.XmlSerialize();

                Hashtable connectionProperties = this.Init();

                // Create a connection to the queue manager using the connection
                // properties just defined
                MQQueueManager qMgr = new MQQueueManager(qManager, connectionProperties);

                // Set up the options on the queue we want to open
                int openOptions = MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_OUTPUT;

                // Now specify the queue that we want to open,and the open options
                //MQQueue system_default_local_queue =
                //  qMgr.AccessQueue("SYSTEM.DEFAULT.LOCAL.QUEUE", openOptions);

                MQQueue system_default_local_queue =
                  qMgr.AccessQueue("DEV.Queue.1", openOptions);

                // Define an IBM MQ message, writing some text in UTF format
                MQMessage hello_world = new MQMessage();
                hello_world.WriteUTF(personXml);

                // Specify the message options
                MQPutMessageOptions pmo = new MQPutMessageOptions(); // accept the defaults,
                                                                     // same as MQPMO_DEFAULT

                // Put the message on the queue
                system_default_local_queue.Put(hello_world, pmo);



                // Get the message back again

                // First define an IBM MQ message buffer to receive the message
                MQMessage retrievedMessage = new MQMessage();
                retrievedMessage.MessageId = hello_world.MessageId;

                // Set the get message options
                MQGetMessageOptions gmo = new MQGetMessageOptions(); //accept the defaults
                                                                     //same as MQGMO_DEFAULT

                // Get the message off the queue
                system_default_local_queue.Get(retrievedMessage, gmo);

                // Prove we have the message by displaying the UTF message text
                string msgText = retrievedMessage.ReadUTF();
                //Console.WriteLine("The message is: {0}", msgText);

                // Close the queue
                system_default_local_queue.Close();

                // Disconnect from the queue manager
                qMgr.Disconnect();
            }

            //If an error has occurred,try to identify what went wrong.

            //Was it an IBM MQ error?
            catch (MQException ex)
            {
                //Console.WriteLine("An IBM MQ error occurred: {0}", ex.ToString());
            }

            catch (System.Exception ex)
            {
                //Console.WriteLine("A System error occurred: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/60171051/ibm-mq-transport-type-managed-work-but-client-does-not-work
        /// </summary>
        /// <returns></returns>
        private Hashtable Init()
        {
            Hashtable connectionProperties = new Hashtable();

            // Add the connection type
            connectionProperties.Add(MQC.TRANSPORT_PROPERTY, connectionType);

            // Set up the rest of the connection properties, based on the
            // connection type requested
            switch (connectionType)
            {
                case MQC.TRANSPORT_MQSERIES_BINDINGS:
                    break;
                case MQC.TRANSPORT_MQSERIES_CLIENT:
                case MQC.TRANSPORT_MQSERIES_XACLIENT:
                case MQC.TRANSPORT_MQSERIES_MANAGED:
                    connectionProperties.Add(MQC.HOST_NAME_PROPERTY, hostName);
                    connectionProperties.Add(MQC.PORT_PROPERTY, "1414");
                    connectionProperties.Add(MQC.CHANNEL_PROPERTY, channel);
                    break;
            }

            return connectionProperties;
        }
    }
}
