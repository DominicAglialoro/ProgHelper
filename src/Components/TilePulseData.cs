using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.ProgHelper;

public class TilePulseData : Component {
    public int[,] TilePulseIndices;

    private VirtualMap<char> tiles;

    private readonly bool bg;

    public TilePulseData(VirtualMap<char> tiles, bool bg) : base(false, false) {
        this.tiles = tiles;
        this.bg = bg;
    }

    public override void EntityAdded(Scene scene) {
        base.EntityAdded(scene);

        if (this.tiles == null)
            return;

        var tiles = this.tiles;

        this.tiles = null;

        var level = SceneAs<Level>();

        if (level == null)
            return;

        var sources = new List<IntVector>();
        int width = tiles.Columns;
        int height = tiles.Rows;

        foreach (var levelData in level.Session.MapData.Levels) {
            foreach (var entityData in levelData.Entities) {
                if (entityData.Name != "progHelper/tilePulseSource" || entityData.Bool("bg") != bg)
                    continue;

                var source = new IntVector(
                    (int) (entityData.Position.X + levelData.Bounds.X - Entity.X) / 8,
                    (int) (entityData.Position.Y + levelData.Bounds.Y - Entity.Y) / 8);

                if (source.X >= 0 && source.X < width && source.Y >= 0 && source.Y < height
                    && tiles[source.X, source.Y] != '0' && tiles[source.X, source.Y] != char.MinValue)
                    sources.Add(source);
            }
        }

        if (sources.Count == 0)
            return;

        TilePulseIndices = new int[width, height];

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++)
                TilePulseIndices[i, j] = -1;
        }

        var toCheck = new Queue<IntVector>();

        foreach (var source in sources) {
            TilePulseIndices[source.X, source.Y] = 0;
            toCheck.Enqueue(source);
        }

        while (toCheck.Count > 0) {
            var vector = toCheck.Dequeue();
            char texture = tiles[vector.X, vector.Y];
            int pulseIndex = TilePulseIndices[vector.X, vector.Y] + 1;

            Check(vector.X - 1, vector.Y);
            Check(vector.X + 1, vector.Y);
            Check(vector.X, vector.Y - 1);
            Check(vector.X, vector.Y + 1);

            void Check(int x, int y) {
                if (x < 0 || x >= width || y < 0 || y >= height || tiles[x, y] != texture || TilePulseIndices[x, y] >= 0)
                    return;

                TilePulseIndices[x, y] = pulseIndex;
                toCheck.Enqueue(new IntVector(x, y));
            }
        }
    }
}