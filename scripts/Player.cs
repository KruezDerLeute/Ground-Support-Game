using Godot;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void CrashedEventHandler();

    [Export] public float MinSpeed = 140f;
    [Export] public float MaxSpeed = 420f;
    [Export] public float Accel = 160f;       // throttle change, px/s per second
    [Export] public float TurnRate = 2.2f;    // radians per second
    [Export] public float StallSpeed = 180f;
    [Export] public float Gravity = 320f;

    public float Speed = 260f;

    // +1 when the plane faces right, -1 when it faces left. This is stored state:
    // working it out from Rotation every frame flickers when the nose is vertical,
    // because cos(Rotation) changes sign there.
    private int _facing = 1;

    private Sprite2D _plane;

    public override void _Ready()
    {
        _plane = GetNode<Sprite2D>("Plane");
        _facing = Mathf.Cos(Rotation) >= 0f ? 1 : -1;
        _plane.FlipV = _facing < 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        float throttle = Input.GetAxis("throttle_down", "throttle_up");
        Speed = Mathf.Clamp(Speed + throttle * Accel * dt, 0, MaxSpeed);

        // Pitch up rotates the nose toward the sky for the way the plane faces.
        // Holding it past vertical carries the plane over the top in a loop.
        float pitch = Input.GetAxis("pitch_down", "pitch_up");
        Rotation = Mathf.Wrap(Rotation - pitch * TurnRate * dt * _facing, -Mathf.Pi, Mathf.Pi);

        Vector2 heading = Vector2.Right.Rotated(Rotation);
        float sag = Gravity * Mathf.Clamp((StallSpeed - Speed) / StallSpeed, 0f, 1f);
        Velocity = heading * Speed + new Vector2(0f, sag);
        MoveAndSlide();

        if (GetSlideCollisionCount() > 0)
            EmitSignal(SignalName.Crashed);
    }
}
