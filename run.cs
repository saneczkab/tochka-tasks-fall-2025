using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    private static readonly Dictionary<char, int> Costs = 
        new() { { 'A', 1 }, { 'B', 10 }, { 'C', 100 }, { 'D', 1000 } };
    private static readonly int[] RoomIdxs = [2, 4, 6, 8];
    private static readonly int[] HallIdxs = [0, 1, 3, 5, 7, 9, 10];
    private static int _roomDepth;
    private const int RoomCount = 4;
    private const int HallLen = 11;
    
    private static int Solve(List<string> lines)
    {
        var startState = ParseStartState(lines);
        return GetMinCost(startState);
    }

    private static string StateToString((string hall, string[] rooms) state) => 
        state.hall + " " + string.Join(" ", state.rooms);
    
    private static (string hall, string[] rooms) StringToState(string stateStr)
    {
        var parts = stateStr.Split(' ');
        var hall = parts[0];
        var rooms = parts[1..];
        return (hall, rooms);
    }
    
    private static int GetMinCost((string hall, string[] rooms) startState)
    {
        var queue = new PriorityQueue<string, int>();
        var foundCosts = new Dictionary<string, int>();
        queue.Enqueue(StateToString(startState),0);

        while (queue.TryDequeue(out var currentStateStr, out _))
        {
            var currentState = StringToState(currentStateStr);
            var currentCost = foundCosts.GetValueOrDefault(currentStateStr, 0);

            if (IsGoalState(currentState))
                return currentCost;

            foreach (var (nextState, moveCost) in GetNextStates(currentState))
            {
                var newCost = currentCost + moveCost;
                var nextStateStr = StateToString(nextState);
                
                if (foundCosts.TryGetValue(nextStateStr, out var oldCost) && oldCost <= newCost)
                    continue;
                
                foundCosts[nextStateStr] = newCost;
                var priority = newCost + Heuristic(nextState);
                queue.Enqueue(nextStateStr, priority);
            }
        }

        return -1;
    }

    private static bool IsGoalState((string hall, string[] rooms) state)
    {
        for (var i = 0; i < RoomCount; i++)
        {
            var room = state.rooms[i];
            var goalChar = (char)('A' + i);

            if (room.Any(c => c != goalChar))
                return false;
        }

        return true;
    }
    
    private static int Heuristic((string hall, string[] rooms) state)
    {
        var heuristic = 0;

        for (var i = 0; i < RoomCount; i++)
        {
            var room = state.rooms[i];
            var door = RoomIdxs[i];

            for (var j = 0; j < _roomDepth; j++)
            {
                var roomObj = room[j];
                if (roomObj == '.')
                    continue;

                if (roomObj ==(char)('A' + i)) 
                    continue;
                
                var goalDoor = RoomIdxs[roomObj - 'A'];
                var steps = j + 1 + Math.Abs(door - goalDoor) + _roomDepth;
                heuristic += steps * Costs[roomObj];
            }
        }

        for (var i = 0; i < HallLen; i++)
        {
            var hallObj = state.hall[i];
            if (hallObj == '.')
                continue;
            
            var goalDoor = RoomIdxs[hallObj - 'A'];
            var steps = Math.Abs(goalDoor - i) + _roomDepth;
            heuristic += steps * Costs[hallObj];
        }

        return heuristic;
    }
    
    private static IEnumerable<((string hall, string[] rooms), int cost)> 
        GetNextStates((string hall, string[] rooms) state)
    {
        foreach (var next in GetNextStatesRoomToHall(state))
            yield return next;
        
        foreach (var next in GetNextStatesHallToRoom(state))
            yield return next;
    }

    
    private static IEnumerable<((string hall, string[] rooms), int cost)> 
        GetNextStatesRoomToHall((string hall, string[] rooms) state)
    {
        for (var i = 0; i < RoomCount; i++)
        {
            var rooms = state.rooms;
            var room = rooms[i];
            var door = RoomIdxs[i];
            
            var top = room.TakeWhile(c => c == '.').Count();
            if (top == _roomDepth) 
                continue;
            
            var topObj = room[top];
            if (topObj == (char)('A' + i) && room.Skip(top).All(c => c == topObj))
                continue;

            foreach (var hallIdx in HallIdxs)
            {
                if (!IsPathClear(state.hall, door, hallIdx))
                    continue;
                
                var steps = top + 1 + Math.Abs(door - hallIdx);
                var cost = steps * Costs[topObj];

                var newHall = state.hall.ToCharArray();
                newHall[hallIdx] = topObj;
                var resultHall = new string(newHall);

                var newRooms = rooms.ToArray();
                var newRoom = room.ToCharArray();
                newRoom[top] = '.';
                newRooms[i] = new string(newRoom);
                
                yield return ((resultHall, newRooms), cost);
            }
        }
    }

    private static IEnumerable<((string hall, string[] rooms), int cost)>
        GetNextStatesHallToRoom((string hall, string[] rooms) state)
    {
        for (var i = 0; i < HallLen; i++)
        {
            var rooms = state.rooms;
            var hall = state.hall;
            var hallObj = hall[i];
            
            if (hallObj == '.')
                continue;
            
            var goalRoomIdx = hallObj - 'A';
            var door = RoomIdxs[goalRoomIdx];
            if(!IsPathClear(hall, i, door))
                continue;
            
            var room = rooms[goalRoomIdx];
            if (room.Any(c => c != '.' && c != hallObj))
                continue;
            
            var roomBottomIdx = room.LastIndexOf('.');
            var steps = roomBottomIdx + 1 + Math.Abs(door - i);
            var cost = steps * Costs[hallObj];
            
            var newHall = hall.ToCharArray();
            newHall[i] = '.';
            var resultHall = new string(newHall);
            
            var newRooms = rooms.ToArray();
            var newRoom = room.ToCharArray();
            newRoom[roomBottomIdx] = hallObj;
            newRooms[goalRoomIdx] = new string(newRoom);
            
            yield return ((resultHall, newRooms), cost);
        }
    }
    
    private static bool IsPathClear(string hall, int start, int end)
    {
        var step = Math.Sign(end - start);
        
        for (var i = start + step; i != end + step; i += step)
        {
            if (hall[i] != '.')
                return false;
        }
        
        return true;
    }
    
    private static (string hall, string[] rooms) ParseStartState(List<string> lines)
    {
        var roomLines = lines[2..^1].Select(line => line.Trim().Replace("#", "")).ToArray();
        _roomDepth = roomLines.Length;
        
        var hall = lines[1][1..^1];
        var rooms = new string[RoomCount];

        for (var i = 0; i < RoomCount; i++)
        {
            var roomState = "";

            for (var j = 0; j < _roomDepth; j++)
            {
                roomState += roomLines[j][i];
            }
            
            rooms[i] = roomState;
        }
        
        return (hall, rooms);
    }
    

    public static void Main()
    {
        var lines = new List<string>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }

        var result = Solve(lines);
        Console.WriteLine(result);
    }
}
