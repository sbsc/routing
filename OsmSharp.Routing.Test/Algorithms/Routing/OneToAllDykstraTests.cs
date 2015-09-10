﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using OsmSharp.Routing.Algorithms.Routing;
using OsmSharp.Routing.Data;
using OsmSharp.Routing.Graphs;
using System;

namespace OsmSharp.Routing.Test.Algorithms.Routing
{
    /// <summary>
    /// Executes tests
    /// </summary>
    [TestFixture]
    class OneToAllDykstraTests
    {
        /// <summary>
        /// Tests shortest path calculations on just one edge.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m----(1) @ 100km/h
        /// </remarks>
        [Test]
        public void TestOneEdge()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
                {
                    Distance = 100,
                    Profile = 1
                }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Speed> getSpeed = (x) =>
            {
                return new Speed()
                {
                    Direction = null,
                    MeterPerSecond = speed
                };
            };

            // run algorithm.
            var algorithm = new OneToAllDykstra(graph, getSpeed, new Path[] { new Path(0) }, 
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Path visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(null, visit.From);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(0, visit.Weight);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(100 / speed, visit.Weight);
        }

        /// <summary>
        /// Tests shortest path calculations with a max value.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m----(1) @ 100km/h
        /// Result:
        ///  Only settle 0 because search is limited to 100m/100km/h
        /// </remarks>
        [Test]
        public void TestMaxValue()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Speed> getSpeed = (x) =>
            {
                return new Speed()
                {
                    Direction = null,
                    MeterPerSecond = speed
                };
            };

            // run algorithm.
            var algorithm = new OneToAllDykstra(graph, getSpeed, new Path[] { new Path(0) },
                (100 / speed) / 2, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Path visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(null, visit.From);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(0, visit.Weight);
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
        }

        /// <summary>
        /// Tests shortest path calculations on just one edge but with a direction.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)----100m----(1) @ 100km/h
        /// Result:
        ///  - Settle 0 and 1 when direction forward.
        ///  - Only settle 0 when direction backward.
        /// </remarks>
        [Test]
        public void TestOneEdgeOneway()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Speed> getSpeed = (x) =>
            {
                return new Speed()
                {
                    Direction = true,
                    MeterPerSecond = speed
                };
            };

            // run algorithm.
            var algorithm = new OneToAllDykstra(graph, getSpeed, new Path[] { new Path(0) },
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Path visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(null, visit.From);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(0, visit.Weight);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(100 / speed, visit.Weight);

            // invert direction.
            getSpeed = (x) =>
            {
                return new Speed()
                {
                    Direction = false,
                    MeterPerSecond = speed
                };
            };

            // run algorithm.
            algorithm = new OneToAllDykstra(graph, getSpeed, new Path[] { new Path(0) },
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(null, visit.From);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(0, visit.Weight);
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
        }
    }
}