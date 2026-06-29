using Godot;
using System;

namespace DeepForest.Scene;

public partial class Forest3D : Node3D
{
    private Camera3D _camera = null!;
    private double _time = 0.0;
    private Vector3 _targetPos;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _targetPos = _camera.Position;
    }

    public override void _Process(double delta)
    {
        _time += delta;
        float bob = (float)Math.Sin(_time * 4.0) * 0.03f;
        
        Vector3 currentTarget = _targetPos;
        currentTarget.Y += bob;
        
        _camera.Position = _camera.Position.Lerp(currentTarget, (float)delta * 5.0f);
    }

    public void StepForward()
    {
        _targetPos += new Vector3(0, 0, -2.0f);
        if (_targetPos.Z < -8.0f)
        {
            _targetPos.Z = 5.0f;
            _camera.Position = new Vector3(_camera.Position.X, _camera.Position.Y, 5.0f);
        }
    }
}
