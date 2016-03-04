using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STAN.Client;

namespace STAN.Client.UnitTests
{
    [TestClass]
    public class UnitTestBasic
    {
        static readonly string CLUSTER_ID="my_test_cluster";
        static readonly string CLIENT_NAME = "me";
        [TestMethod]
        public void TestNoServer()
        {
            try
            {
                IConnection c = new STAN.Client.StanConnectionFactory().CreateConnection(CLUSTER_ID, CLIENT_NAME);
            }
            catch (STANBadConnectionException)
            {
                ;; //Expected
            }
        }
    }
}
