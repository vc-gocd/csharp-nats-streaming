using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace STAN.Client
{
    public class InternalProtoTesting
    {
        MemoryStream ms = new MemoryStream();
        Google.Protobuf.CodedOutputStream outStream = null;
        Google.Protobuf.CodedInputStream inStream = null;


        public InternalProtoTesting()
        {
            outStream = new CodedOutputStream(ms);
            inStream = new CodedInputStream(ms);
        }

        public void Test()
        {
            /* yikes - doesn't work! */
            STAN.Client.ConnectRequest inCr = new ConnectRequest();
            STAN.Client.ConnectRequest outCr = new ConnectRequest();
            
            inCr.ClientID = "me";

            testObject(inCr, outCr);

            STAN.Client.Ack inAck = new Ack();
            STAN.Client.Ack outAck = new Ack();

            inAck.Sequence = 9;
            testObject(inAck, outAck);

            STAN.Client.CloseRequest inCloseReq = new CloseRequest();
            STAN.Client.CloseRequest outCloseReq = new CloseRequest();
            inCloseReq.ClientID = "foo";

            testObject(inCloseReq, outCloseReq);

            STAN.Client.CloseResponse inCloseResponse = new CloseResponse();
            STAN.Client.CloseResponse outCloseResponse = new CloseResponse();

            inCloseResponse.Error = "value";
            testObject(inCloseResponse, outCloseResponse);

            STAN.Client.SubscriptionRequest inSubReq = new SubscriptionRequest();
            STAN.Client.SubscriptionRequest outSubReq = new SubscriptionRequest();

            inSubReq.AckWaitInSecs = 5;
            inSubReq.ClientID = "foo";
            inSubReq.DurableName = "durname";
            inSubReq.Inbox = "inbox";
            inSubReq.MaxInFlight = 5000;
            inSubReq.QGroup = "qgroup";
            inSubReq.StartPosition = new StartPosition();
            inSubReq.StartSequence = 99;
            //inSubReq.StartTime = 100000;
            inSubReq.Subject = "subject";

            testObject(inSubReq, outSubReq);

            STAN.Client.SubscriptionResponse inSr = new SubscriptionResponse();
            STAN.Client.SubscriptionResponse outSr = new SubscriptionResponse();

            inSr.AckInbox = "inbox";
            inSr.Error = "error";

            testObject(inSr, outSr);

            STAN.Client.ConnectRequest inConnectReq = new ConnectRequest();
            STAN.Client.ConnectRequest outConnectReq = new ConnectRequest();

            inConnectReq.ClientID = "id";

            testObject(inConnectReq, outConnectReq);

            STAN.Client.ConnectResponse inConnectResp = new ConnectResponse();
            STAN.Client.ConnectResponse outConnectResp = new ConnectResponse();


            inConnectResp.CloseRequests = "sdf";
            inConnectResp.Error = "error";
            testObject(inConnectReq, outConnectResp);

            PubMsg inMsg = new PubMsg();
            PubMsg outMsg = new PubMsg();
            //inMsg.Id = "id";
            //inMsg.Reply = "reply";
            //inMsg.Subject = "subject";
           // inMsg.Sha256 = Google.Protobuf.ByteString.CopyFromUtf8("testString");
            //inMsg.Data = Google.Protobuf.ByteString.CopyFromUtf8("testString");

            testObject(inMsg, outMsg);

            MsgProto inMsgProto = new MsgProto();
            MsgProto outMsgProto = new MsgProto();

            inMsgProto.CRC32 = 1234;
            inMsgProto.Data = Google.Protobuf.ByteString.CopyFromUtf8("payload");
            inMsgProto.Redelivered = true;
            inMsgProto.Reply = "reply";
            inMsgProto.Sequence = 123;
            inMsgProto.Subject = "subject";
            inMsgProto.Timestamp = 567;

            testObject(inMsgProto, outMsgProto);
        }

        private void testObject(IMessage inMsg, IMessage outMsg)
        {
            try
            {
                System.Console.WriteLine("Testing type {0}", inMsg.GetType().FullName);

                System.Console.WriteLine(inMsg);

                ms.Position = 0;

                outStream.WriteMessage(inMsg);
                outStream.Flush();

                ms.Position = 0;

                inStream.ReadMessage(outMsg);

                System.Console.WriteLine(outMsg);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                System.Console.WriteLine("{0} Failed to parse!", inMsg.GetType().FullName);
            }
        }
    }
}
