using System;
using System.Collections.Generic;

namespace Project3InfluenceScore
{
   
    class Graph
    {
        //adjacency list
        //each node maps to a list of (neighbour, weight)
        private Dictionary<string, List<(string neighbour, int weight)>> adjList;

        // flag to track whether the graph is weighted
        private bool isWeighted;

        public Graph(bool weighted)
        {
            adjList = new Dictionary<string, List<(string, int)>>();
            isWeighted = weighted;
        }

        //add an undirected edge
        //for unweighted graphs, weight is always set to 1

        public void AddEdge(string u, string v, int weight = 1)
        {
            //if node doesnt exist create it
            if (!adjList.ContainsKey(u))
                adjList[u] = new List<(string, int)>();

            if (!adjList.ContainsKey(v))
                adjList[v] = new List<(string, int)>();
            //decides final weight
            int finalWeight = isWeighted ? weight : 1;
        //add edge in both direcrtion
            adjList[u].Add((v, finalWeight));
            adjList[v].Add((u, finalWeight));
        }

        //public method to calculate influence score
        //automatically chooses BFS or Dijkstra
        public double CalculateInfluenceScore(string source)
        {
            //use dijkstras
            if (isWeighted)
                return InfluenceScoreWeighted(source);
            //use BFS
            else
                return InfluenceScoreUnweighted(source);
        }

        // BFS for unweighted networks
        private double InfluenceScoreUnweighted(string source)
        {
            var distances = new Dictionary<string, int>();
            var queue = new Queue<string>();
            //all distance infinite
            foreach (var node in adjList.Keys)
                distances[node] = int.MaxValue;
            //distance to source is zero
            distances[source] = 0;
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var (neighbour, _) in adjList[current])
                {
                    if (distances[neighbour] == int.MaxValue)
                    {
                        distances[neighbour] = distances[current] + 1;
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return ComputeInfluenceScore(source, distances);
        }

        //dijkstra for weighted networks
        private double InfluenceScoreWeighted(string source)
        {
            var distances = new Dictionary<string, int>();
            var pq = new PriorityQueue<string, int>();

            foreach (var node in adjList.Keys)
                distances[node] = int.MaxValue;

            distances[source] = 0;
            pq.Enqueue(source, 0);

            while (pq.Count > 0)
            {
                var current = pq.Dequeue();

                foreach (var (neighbour, weight) in adjList[current])
                {
                    int newDist = distances[current] + weight;

                    if (newDist < distances[neighbour])
                    {
                        distances[neighbour] = newDist;
                        pq.Enqueue(neighbour, newDist);
                    }
                }
            }

            return ComputeInfluenceScore(source, distances);
        }

        //influence score formula
        //(n - 1) / sum of shortest path distances
        private double ComputeInfluenceScore(string source, Dictionary<string, int> distances)
        {
            double sum = 0;
            int n = adjList.Count;

            foreach (var node in distances.Keys)
            {
                if (node != source)
                {
                    if (distances[node] == int.MaxValue)
                        return 0; // unreachable node

                    sum += distances[node];
                }
            }

            return (n - 1) / sum;
        }
    }

    //my main Program with menu and test demonstrations
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Influence Score Calculator");
            Console.WriteLine("1. Run weighted network");
            Console.WriteLine("2. Run unweighted network");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine();

            if (choice == "1")
                RunWeightedNetwork();
            else if (choice == "2")
                RunUnweightedNetwork();
            else
                Console.WriteLine("Invalid selection.");
        }

        // Weighted network from the brief
        static void RunWeightedNetwork()
        {
            Console.WriteLine("\nRunning weighted network");

            var graph = new Graph(weighted: true);

            graph.AddEdge("A", "B", 1);
            graph.AddEdge("A", "C", 1);
            graph.AddEdge("A", "E", 5);
            graph.AddEdge("B", "C", 4);
            graph.AddEdge("B", "E", 1);
            graph.AddEdge("B", "G", 1);
            graph.AddEdge("B", "H", 1);
            graph.AddEdge("C", "D", 3);
            graph.AddEdge("C", "E", 1);
            graph.AddEdge("D", "E", 2);
            graph.AddEdge("D", "F", 1);
            graph.AddEdge("D", "G", 5);
            graph.AddEdge("E", "G", 2);
            graph.AddEdge("F", "G", 1);
            graph.AddEdge("G", "H", 2);
            graph.AddEdge("H", "I", 3);
            graph.AddEdge("I", "J", 3);

            double score = graph.CalculateInfluenceScore("A");

            Console.WriteLine($"Influence score for node A: {score}");
        }

        //unweighted social network from the brief
        static void RunUnweightedNetwork()
        {
            Console.WriteLine("\nRunning unweighted network...");

            var graph = new Graph(weighted: false);

            graph.AddEdge("Alicia", "Britney");
            graph.AddEdge("Britney", "Claire");
            graph.AddEdge("Claire", "Diana");
            graph.AddEdge("Diana", "Edward");
            graph.AddEdge("Diana", "Harry");
            graph.AddEdge("Edward", "Harry");
            graph.AddEdge("Edward", "Gloria");
            graph.AddEdge("Edward", "Fred");
            graph.AddEdge("Gloria", "Fred");
            graph.AddEdge("Harry", "Gloria");

            double score = graph.CalculateInfluenceScore("Edward");

            Console.WriteLine($"Influence score for Edward: {score}");
        }
    }
}
