using System;
using MemoryCache;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryCache.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Capacity_Expect_CorrectCapacity()
        {
            var cache = new Cache<int, int>(capacity: 100);
            Assert.That(cache.Capacity, Is.EqualTo(100));
        }

        [Test]
        public void IsFull_When_NotFull_Expect_False()
        {
            var cache = new Cache<int, int>(capacity:5);
            cache.AddOrUpdate(0, 10);
            cache.AddOrUpdate(1, 20);
            cache.AddOrUpdate(2, 30);
            cache.AddOrUpdate(3, 40);
            Assert.False(cache.IsFull);
        }

        [Test]
        public void IsFull_When_Full_Expect_True()
        {
            var cache = new Cache<int, int>(capacity: 5);
            cache.AddOrUpdate(0, 10);
            cache.AddOrUpdate(1, 20);
            cache.AddOrUpdate(2, 30);
            cache.AddOrUpdate(3, 40);
            cache.AddOrUpdate(4, 50);
            Assert.True(cache.IsFull);
        }

        [Test]
        public void Count_Expect_CorrectCount()
        {
            var cache = new Cache<int, int>(capacity: 100);
            cache.AddOrUpdate(0, 10);
            cache.AddOrUpdate(1, 20);
            cache.AddOrUpdate(2, 30);
            cache.AddOrUpdate(3, 40);
            cache.AddOrUpdate(4, 50);
            Assert.That(cache.Count, Is.EqualTo(5));
            cache.AddOrUpdate(5, 60);
            Assert.That(cache.Count, Is.EqualTo(6));

        }

        [Test]
        public void Count_When_CacheFull_Expect_CorrectCount()
        {
            var cache = new Cache<int, int>(capacity: 5);
            cache.AddOrUpdate(0, 10);
            cache.AddOrUpdate(1, 20);
            cache.AddOrUpdate(2, 30);
            cache.AddOrUpdate(3, 40);
            cache.AddOrUpdate(4, 50);
            Assert.That(cache.Count, Is.EqualTo(5));
            cache.AddOrUpdate(5, 60);
            Assert.That(cache.Count, Is.EqualTo(5));

        }

        [Test]
        public void AddOrUpdate_When_CacheEmpty_Expect_ValueEqualsOutValueInCache()
        {
            var cache = new Cache<int, string>(capacity: 100);
            var key = 0;
            var value = "First Item";
            cache.AddOrUpdate(key, value);

            string valueInCache;
            Assert.True(cache.TryGetValue(key, out valueInCache));
            Assert.AreEqual(value, valueInCache, "Value in cache does not match value put in");
        }

        [Test]
        public void TryGetValue_Expect_FetchCorrectValue()
        {
            var cache = new Cache<int, string>(capacity:100);
            var key = 3;
            var value = "Item 4";
            string valueInCache;

            cache.AddOrUpdate(0, "Item 1");
            cache.AddOrUpdate(1, "Item 2");
            cache.AddOrUpdate(2, "Item 3");
            cache.AddOrUpdate(3, "Item 4");
            cache.AddOrUpdate(4, "Item 4");
            cache.AddOrUpdate(5, "Item 4");

            Assert.True(cache.TryGetValue(key, out valueInCache));
            Assert.AreEqual(value, valueInCache, "Value in cache does not match value put in");
        }


        [Test]
        //Adding new node with capacity of 5
        //Expect oldest node (0, "Item 1"), to be removed
        //and new one (5, "Item 6")to be added to the top of the cache
        public void TryGetValue_When_CacheFull_Expect_FetchNewAddedValue()
        {
            var cache = new Cache<int, string>(capacity:5);
            var key = 5;
            var value = "Item 6";
            string valueInCache;

            cache.AddOrUpdate(0, "Item 1");
            cache.AddOrUpdate(1, "Item 2");
            cache.AddOrUpdate(2, "Item 3");
            cache.AddOrUpdate(3, "Item 4");
            cache.AddOrUpdate(4, "Item 5");
            cache.AddOrUpdate(5, "Item 6");

            Assert.True(cache.TryGetValue(key, out valueInCache));
            Assert.AreEqual(value, valueInCache, "Value in cache does not match value put in");
        }

        [Test]
        //Adding new node when cache is full (capacity = 5)  which now makes (0, "Item 1") the latest node added
        //Expect oldest node (0, "Item 1"), to be removed
        //and new one (5, "Item 6")to be added to the top of the cache
        public void TryGetValue_When__AddingNode_Expect_O1_OldestNodeRemoved()
        {
            var cache = new Cache<int, string>(capacity: 5);
            var key = 0;
            string value = "Item 1";
            string valueInCache;

            cache.AddOrUpdate(0, "Item 1");
            cache.AddOrUpdate(1, "Item 2");
            cache.AddOrUpdate(2, "Item 3");
            cache.AddOrUpdate(3, "Item 4");
            cache.AddOrUpdate(4, "Item 5");
            cache.AddOrUpdate(5, "Item 6");

            Assert.False(cache.TryGetValue(key, out valueInCache));
            Assert.AreNotEqual(value, valueInCache, "Value put in should not exist in cache");
        }

        [Test]
        //Retrieving all existing nodes but node (3, "Item 4)", which now become the latest retrieved
        //Expect oldest node (3, "Item 4"), to be removed
        //and new one (5, "Item 6")to be added to the top of the cache
        public void TryGetValue_When_RetrievingNode_Expect_O1_OldestNodeRemoved()
        {
            var cache = new Cache<int, string>(capacity: 5);
            var key = 3;
            string value = "Item 4";
            string valueInCache;

            cache.AddOrUpdate(0, "Item 1");
            cache.AddOrUpdate(1, "Item 2");
            cache.AddOrUpdate(2, "Item 3");
            cache.AddOrUpdate(3, "Item 4");
            cache.AddOrUpdate(4, "Item 5");
            
            cache.TryGetValue(0, out valueInCache);
            cache.TryGetValue(1, out valueInCache);
            cache.TryGetValue(2, out valueInCache);
            cache.TryGetValue(4, out valueInCache);

            cache.AddOrUpdate(5, "Item 6");

            Assert.False(cache.TryGetValue(key, out valueInCache));
            Assert.AreNotEqual(value, valueInCache, "Value put in should not exist in cache");
        }

        [Test]
        //Updating all existing nodes but node (3, "Item 4)", which now become the latest updated
        //Expect oldest node (3, "Item 4"), to be removed
        //and new one (6, "Item 7")to be added to the top of the cache
        public void TryGetValue_When_UpdatingNode_Expect_O1_OldestNodeRemoved()
        {
            var cache = new Cache<int, string>(capacity: 5);
            var key = 3;
            string value = "Item 4";
            string valueInCache;

            cache.AddOrUpdate(0, "Item 1");
            cache.AddOrUpdate(1, "Item 2");
            cache.AddOrUpdate(2, "Item 3");
            cache.AddOrUpdate(3, "Item 4");
            cache.AddOrUpdate(4, "Item 5");

            cache.AddOrUpdate(0, "Node 1");
            cache.AddOrUpdate(1, "Node 2");
            cache.AddOrUpdate(2, "Node 3");
            cache.AddOrUpdate(4, "Node 5");

            cache.AddOrUpdate(5, "Node 6");

            Assert.False(cache.TryGetValue(key, out valueInCache));
            Assert.AreNotEqual(value, valueInCache, "Value put in should not exist in cache");
        }

        [Test]
        //Updating existing node (same key, different value) a few times
        //Expect value of the node to be the lastest updated one
        public void AddOrUpdate_When_KeyIdentical_Expect_ValueToBeUpdated()
        {
            var cache = new Cache<int, string>(capacity: 5);
            var key = 0;
            string MostUpdatedValue = "Last update for initial node";
            string valueInCache;

            cache.AddOrUpdate(0, "Initial node");
            cache.AddOrUpdate(0, "Same node updated");
            cache.AddOrUpdate(0, MostUpdatedValue);

            Assert.True(cache.TryGetValue(key, out valueInCache));
            Assert.AreEqual(MostUpdatedValue, valueInCache, "Value in cache does not match value put in");
        }
    }
}
