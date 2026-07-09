using Godot;
using System;

namespace DeepForest.Scene;

public partial class SceneEffect2D : Node
{
    private Control? _targetNode;
    private double _time = 0.0;
    private Vector2 _basePosition;
    private float _shakeIntensity = 0.0f;
    private double _shakeTimer = 0.0;

    public void Initialize(Control targetNode)
    {
        _targetNode = targetNode;
        _basePosition = targetNode.Position;
    }

    public override void _Process(double delta)
    {
        if (_targetNode == null) return;

        _time += delta;

        // Bobbing effect (breathing/walking simulation)
        float bobX = (float)Math.Sin(_time * 2.0) * 1.5f;
        float bobY = (float)Math.Cos(_time * 4.0) * 1.0f;

        Vector2 offset = new Vector2(bobX, bobY);

        // Shake effect
        if (_shakeTimer > 0)
        {
            _shakeTimer -= delta;
            float rx = (Random.Shared.NextSingle() * 2.0f - 1.0f) * _shakeIntensity;
            float ry = (Random.Shared.NextSingle() * 2.0f - 1.0f) * _shakeIntensity;
            offset += new Vector2(rx, ry);
        }

        _targetNode.Position = _basePosition + offset;
    }

    public void StepForward()
    {
        Shake(5.0f, 0.3f);
    }

    public void Shake(float intensity, float duration)
    {
        _shakeIntensity = intensity;
        _shakeTimer = duration;
    }
}
