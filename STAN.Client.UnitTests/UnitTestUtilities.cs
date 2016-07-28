/*******************************************************************************
 * Copyright (c) 2015-2016 Apcera Inc. All rights reserved. This program and the accompanying
 * materials are made available under the terms of the MIT License (MIT) which accompanies this
 * distribution, and is available at http://opensource.org/licenses/MIT
 *******************************************************************************/
using System;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace STAN.Client.UnitTests
{
    class NatsStreamingServer : IDisposable
    {
        // Enable this for additional server debugging info.
        bool debug = false;
        Process p;

        private bool isNatsServerRunning()
        {
            try
            {
                NATS.Client.IConnection c = new NATS.Client.ConnectionFactory().CreateConnection();
                c.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void init(bool shouldDebug)
        {
            debug = shouldDebug;
            ProcessStartInfo psInfo = createProcessStartInfo();
            this.p = Process.Start(psInfo);
            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(500);
                if (isNatsServerRunning())
                    break;
            }
            // Allow the Nats streaming server to setup.
            Thread.Sleep(250);
        }

        public NatsStreamingServer()
        {
            init(false);
        }

        public NatsStreamingServer(bool shouldDebug)
        {
            init(shouldDebug);
        }

        private void addArgument(ProcessStartInfo psInfo, string arg)
        {
            if (psInfo.Arguments == null)
            {
                psInfo.Arguments = arg;
            }
            else
            {
                string args = psInfo.Arguments;
                args += arg;
                psInfo.Arguments = args;
            }
        }

        public NatsStreamingServer(int port)
        {
            ProcessStartInfo psInfo = createProcessStartInfo();

            addArgument(psInfo, "-p " + port);

            this.p = Process.Start(psInfo);
        }

        public NatsStreamingServer(string args)
        {
            ProcessStartInfo psInfo = this.createProcessStartInfo();
            addArgument(psInfo, args);
            p = Process.Start(psInfo);
        }

        private ProcessStartInfo createProcessStartInfo()
        {
            string nss = "nats-streaming-server";
            ProcessStartInfo psInfo = new ProcessStartInfo(nss);

            if (debug)
            {
                psInfo.Arguments = " -SDV -DV";
            }
            else
            {
                psInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            psInfo.WorkingDirectory = UnitTestUtilities.GetConfigDir();

            return psInfo;
        }

        public void Shutdown()
        {
            if (p == null)
                return;

            try
            {
                p.Kill();
            }
            catch (Exception) { }

            p = null;
        }

        void IDisposable.Dispose()
        {
            Shutdown();
        }
    }

    class ConditionalObj
    {
        Object objLock = new Object();
        bool completed = false;

        internal void wait(int timeout)
        {
            lock (objLock)
            {
                if (completed)
                    return;

                Assert.True(Monitor.Wait(objLock, timeout));
            }
        }

        internal void reset()
        {
            lock (objLock)
            {
                completed = false;
            }
        }

        internal void notify()
        {
            lock (objLock)
            {
                completed = true;
                Monitor.Pulse(objLock);
            }
        }
    }

    class UnitTestUtilities
    {
        Object mu = new Object();
        static NatsStreamingServer defaultServer = null;
        Process authServerProcess = null;

        static internal string GetConfigDir()
        {
            string baseDir = Assembly.GetExecutingAssembly().CodeBase;
            return baseDir + "\\NATSUnitTests\\config";
        }

        public void StartDefaultServer()
        {
            lock (mu)
            {
                if (defaultServer == null)
                {
                    defaultServer = new NatsStreamingServer();
                }
            }
        }

        public void StopDefaultServer()
        {
            lock (mu)
            {
                try
                {
                    defaultServer.Shutdown();
                }
                catch (Exception) { }

                defaultServer = null;
            }
        }

        public void bounceDefaultServer(int delayMillis)
        {
            StopDefaultServer();
            Thread.Sleep(delayMillis);
            StartDefaultServer();
        }

        public void startAuthServer()
        {
            authServerProcess = Process.Start("stan-server -config auth.conf");
        }

        internal static void testExpectedException(Action call, Type exType)
        {
            try
            {
                call.Invoke();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                Assert.IsAssignableFrom(exType, e);
                return;
            }

            Assert.True(false, "No exception thrown!");
        }

        internal NatsStreamingServer CreateServerOnPort(int p)
        {
            return new NatsStreamingServer(p);
        }

        internal NatsStreamingServer CreateServerWithConfig(string configFile)
        {
            return new NatsStreamingServer(" -config " + configFile);
        }

        internal NatsStreamingServer CreateServerWithArgs(string args)
        {
            return new NatsStreamingServer(" " + args);
        }

        internal static String GetFullCertificatePath(string certificateName)
        {
            return GetConfigDir() + "\\certs\\" + certificateName;
        }

        internal static void CleanupExistingServers()
        {
            try
            {
                Process[] procs = Process.GetProcessesByName("stan-server");

                foreach (Process proc in procs)
                {
                    proc.Kill();
                }
            }
            catch (Exception) { } // ignore
        }
    }
}
