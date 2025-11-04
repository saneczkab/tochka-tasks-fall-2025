using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();
        var graph = ParseGraph(edges);
        var start = "a";

        while (true)
        {
            var path = GetShotestGatewayPath(graph, start);
            if (path.Count == 0)
            {
                break;
            }
            
            var gateway = path.Last();
            var prevNode = path[^2];
            result.Add($"{gateway}-{prevNode}");

            graph[gateway].Remove(prevNode);
            graph[prevNode].Remove(gateway);
            
            if (!IsAnyGatewayReachable(graph, start))
            {
                break;
            }
            
            if (path.Count > 2) 
            {
                start = path[1];
            }
        }
        
        return result;
    }
    
    private static List<string> GetShotestGatewayPath(Dictionary<string, List<string>> graph, string start)
    {
        var queue = new Queue<List<string>>();
        var visited = new HashSet<string>();
        queue.Enqueue([start]);
        visited.Add(start);
        var result = new List<string>();

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var node = path.Last();
            var neighbors = graph[node].OrderBy(n => n);
            
            foreach (var neighbor in neighbors)
            {
                if (char.IsUpper(neighbor[0]))
                {
                    var newPath = path.Append(neighbor).ToList();
                    if (result.Count == 0 || newPath.Count < result.Count || (newPath.Count == result.Count && 
                        string.Compare(newPath.Last(), result.Last(), StringComparison.Ordinal) < 0))
                    {
                        result = newPath;
                    }
                }
                else
                {
                    if (visited.Add(neighbor))
                    {
                        var newPath = path.Append(neighbor).ToList();
                        queue.Enqueue(newPath);
                    }
                }
            }
        }
        
        return result;
    }
    
    private static bool IsAnyGatewayReachable(Dictionary<string, List<string>> graph, string start)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var neighbors = graph[node];

            foreach (var neighbor in neighbors)
            {
                if (char.IsUpper(neighbor[0]))
                {
                    return true;
                }
                
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return false;
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