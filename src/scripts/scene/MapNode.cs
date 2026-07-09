using System;
using System.Collections.Generic;

namespace DeepForest.Scene
{
    public class MapNode
    {
        public int Id { get; set; }
        public int Depth { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; } = "";
        public SceneData SceneData { get; set; } = new();
        public List<int> Connections { get; set; } = new();
    }
}
