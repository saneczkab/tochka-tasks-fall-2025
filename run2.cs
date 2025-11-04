using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();
        var graph = ParseGraph(edges);
        var gatewayData = GetGatewaysData(graph);
        var depths = new List<int>(gatewayData.Keys);
        depths.Sort();

        foreach (var depth in depths)
        {
            var resultEdges = gatewayData[depth]
                .OrderBy(edge => edge.gateway)
                .ThenBy(edge => edge.lastNode);
            
            foreach (var (gateway, lastNode) in resultEdges)
            {
                var edgeStr = $"{gateway}-{lastNode}";
                result.Add(edgeStr);
            }
        }

        return result;
    }

    private static Dictionary<int, List<(string gateway, string lastNode)>> 
        GetGatewaysData(Dictionary<string, List<string>> graph, string start = "a")
    {
        var result = new Dictionary<int, List<(string gateway, string lastNode)>>();
        var visited = new HashSet<string>();
        var queue = new Queue<(string node, int depth)>();
        
        queue.Enqueue((start, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (currentNode, depth) = queue.Dequeue();
            var neighbors = graph[currentNode];

            foreach (var neighbor in neighbors)
            {
                if (char.IsUpper(neighbor[0]))
                {
                    if (!result.ContainsKey(depth))
                    {
                        result[depth] = new List<(string gateway, string lastNode)>();
                    }
                    
                    result[depth].Add((neighbor, currentNode));
                }
                else
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue((neighbor, depth + 1));
                    }
                }
            }
        }
        
        return result;
    }

    private static Dictionary<string, List<string>> ParseGraph(List<(string, string)> edges)
    {
        var graph = new Dictionary<string, List<string>>();

        foreach (var (u, v) in edges)
        {
            if (!graph.ContainsKey(u))
                graph[u] = new List<string>();
            if (!graph.ContainsKey(v))
                graph[v] = new List<string>();

            graph[u].Add(v);
            graph[v].Add(u);
        }
        
        return graph;
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }

        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}