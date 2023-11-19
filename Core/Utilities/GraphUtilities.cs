using System.Collections.Generic;
using System.Linq;

namespace PolyArchitect.Core {

// various graph algorithms
// implemented with integer nodes
public static class GraphUtilities {
    public static HashSet<(int, int)> EdgesOfCircularList(List<int> nodes) {
        var set = new HashSet<(int, int)>();
        for (int i = 0; i < nodes.Count; i++) {
            set.Add((nodes[i], nodes[(i+1) % nodes.Count]));
        }

        return set;
    }

    // each node is represented by an integer
    // a graph is circular if:
    // - number of edges is number of vertices
    // - each vertex has degree 2
    public static bool CheckCircularGraph(HashSet<(int, int)> edges, HashSet<int> nodes) {
        if (edges.Count != nodes.Count){
            return false;
        }

        var nodeDegrees = new Dictionary<int, int>();
        foreach (var node in nodes) {
            nodeDegrees.Add(node, 0);
        }

        foreach (var edge in edges) {
            nodeDegrees[edge.Item1] += 1;
            nodeDegrees[edge.Item2] += 1;
        }

        var allDegreesAreTwo = nodeDegrees.Values.All((degree) => degree == 2);

        if (allDegreesAreTwo) {
            return true;
        } else {
            return false;
        }
    }

    public static Dictionary<int, HashSet<int>> MakeAdjacencyList(HashSet<(int, int)> edges, HashSet<int> nodes) {
        var adjacencyList = new Dictionary<int, HashSet<int>>();

        foreach (var node in nodes) {
            adjacencyList.Add(node, new HashSet<int>());
        }

        foreach (var edge in edges) {
            adjacencyList[edge.Item1].Add(edge.Item2);
            adjacencyList[edge.Item2].Add(edge.Item1);
        }

        return adjacencyList;
    }

    // input is convex polygon
    public static List<int> GetLoopOrder(HashSet<(int, int)> edges, HashSet<int> nodes){
        var adjacencyList = MakeAdjacencyList(edges, nodes);

        var firstNode = nodes.First();
        var currentNode = firstNode;
        var prevNode = adjacencyList[currentNode].First();

        var loop = new List<int>();

        var iterations = 0;

        do {
            loop.Add(currentNode);

            var nextNode = adjacencyList[currentNode].Where((node) => node != prevNode).First();
            prevNode = currentNode;
            currentNode = nextNode;

            if (iterations >= adjacencyList.Count) {
                throw new System.Exception("graph is not a convex polygon");
            }
            iterations += 1;
        } while(currentNode != firstNode);

        return loop;
    }

}
}