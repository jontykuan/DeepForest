using Godot;
using System.Collections.Generic;
using DeepForest.Rendering;

namespace DeepForest.UI
{
    public class MapPanel
    {
        private readonly RichTextLabel _mapLabel;

        public MapPanel(RichTextLabel mapLabel)
        {
            _mapLabel = mapLabel;
        }

        public void UpdateMap(MapRenderer renderer, int currentNodeId, HashSet<int> exploredNodeIds)
        {
            if (_mapLabel == null) return;
            var mapGrid = renderer.RenderMap(currentNodeId, exploredNodeIds);
            _mapLabel.Text = mapGrid.ToBBCode();
        }
    }
}
