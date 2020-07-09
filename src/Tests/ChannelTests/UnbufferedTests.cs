// Test code based on source from Chan4Net:
// https://github.com/superopengl/Chan4Net
// Copyright (c) 2016 Jun Shao

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace ChannelTests
{
    [TestClass]
    public class UnbufferedTests
    {
        private readonly TimeSpan m_awaitTimeout = TimeSpan.FromMilliseconds(100);
        private readonly TimeSpan m_slowActionLatency = TimeSpan.FromMilliseconds(100);

        [TestMethod]
        public void Receive_ShouldBeBlocked_IfNoOneSend()
        {
            channel<int> chan = new channel<int>(1);
            bool called = false;
            Task task = Task.Run(() =>
            {
                chan.Receive();
                called = true;
            });

            task.Wait(m_awaitTimeout);

            Assert.IsFalse(called);
        }

        [TestMethod]
        public void Receive_ShouldGetTheSameObjectThatSenderSent()
        {
            channel<object> chan = new channel<object>(1);
            object item = new object();

            Task receiver = Task.Run(() =>
            {
                Thread.Sleep(m_slowActionLatency);
                chan.Send(item);
            });

            object result = chan.Receive();
            receiver.Wait();

            Assert.AreSame(item, result);
        }

        [TestMethod]
        public void Receive_ShouldNotBeBlocked_OnceOneSend()
        {
            channel<int> chan = new channel<int>(1);
            bool called = false;
            int item = 0;
            Task receiver = Task.Run(() =>
            {
                item = chan.Receive();
                called = true;
            });

            Thread.Sleep(m_slowActionLatency);
            chan.Send(1); // Make sure Receive() is called before Send()

            receiver.Wait();

            Assert.AreEqual(1, item);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void Receive_RaceCondition_OnlyOneCanGetTheSentItemEachTime()
        {
            channel<int> chan = new channel<int>(1);
            ConcurrentBag<int> items = new ConcurrentBag<int>();
            Task receiver1 = Task.Run(() => items.Add(chan.Receive()));
            Task receiver2 = Task.Run(() => items.Add(chan.Receive()));
            Task receiver3 = Task.Run(() => items.Add(chan.Receive()));

            Thread.Sleep(m_slowActionLatency);
            chan.Send(1);

            Task.WaitAll(new[] { receiver1, receiver2, receiver3 }, m_awaitTimeout);

            CollectionAssert.AreEquivalent(new[] { 1 }, items.ToArray());
        }

        [TestMethod]
        public void Receive_RaceCondition_OneItemIsReceivedOnlyOnce()
        {
            channel<int> chan = new channel<int>(1);
            ConcurrentBag<int> items = new ConcurrentBag<int>();
            int[] samples = Enumerable.Range(0, 3).ToArray();
            Task[] receivers = samples.Select(i => { return Task.Run(() => items.Add(chan.Receive())); }).ToArray();

            Thread.Sleep((int)m_slowActionLatency.TotalMilliseconds);
            foreach (int i in samples)
            {
                chan.Send(i);
            }

            Task.WaitAll(receivers);

            CollectionAssert.AreEquivalent(samples, items.ToArray());
        }

        [TestMethod]
        public void Receive_NoSend_NoBufferedChan_ShouldBlock()
        {
            channel<int> sut = new channel<int>(1);
            bool called = false;
            Task receiver = Task.Run(() =>
            {
                sut.Receive();
                called = true;
            });

            receiver.Wait(m_awaitTimeout);

            Assert.IsFalse(called);
        }

        // TODO: Reenable test when deadlock checks have been enabled
        //[TestMethod]
        //public void Send_NoReceiver_NoBufferedChan_ShouldThrow()
        //{
        //    channel<int> sut = new channel<int>(1);
        //    Exception exception = null;
        //    try
        //    {
        //        sut.Send(1);
        //    }
        //    catch (Exception ex)
        //    {
        //        exception = ex;
        //    }

        //    Assert.IsInstanceOfType(exception, typeof(InvalidOperationException));
        //}
    }
}