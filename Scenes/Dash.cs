using Godot ;
using System ;

public class Dash : Spatial
{
  private const string DurationTimerPath = "DurationTimer" ;
  private const string DashEffectScenePath = "res://Scenes/DashEffect.tscn" ;
  private const string TimeOutEventName = "timeout" ;
  private Timer _durationTimer ;
  private bool _canDash = true ;
  private PackedScene _dashEffectResource ;

  public bool CanDash
  {
    get => _canDash && ! IsDashing ;
    private set => _canDash = value ;
  }

  private const float DashDelay = 0.4f ;
  public bool IsDashing => ! _durationTimer.IsStopped() ;

  public override void _Ready()
  {
    _durationTimer = GetNode<Timer>( DurationTimerPath ) ;
    _durationTimer.OneShot = true ;
    _durationTimer.Connect( TimeOutEventName, this, nameof( OnDurationTimer_TimeOut ) ) ;
    _dashEffectResource = GD.Load( DashEffectScenePath ) as PackedScene ;
    base._Ready() ;
  }

  public void StartDash( float duration )
  {
    _durationTimer.WaitTime = duration ;
    _durationTimer.Start() ;
    InstanceEffect( ) ;
  }

  private void InstanceEffect( )
  {
    if ( ! ( _dashEffectResource.Instance() is Sprite effect ) ) return ;
    GetParent().GetParent().AddChild( effect ) ;
    effect.GlobalPosition = new Vector2( GlobalTransform.origin.x, GlobalTransform.origin.y ) ;
    // effect.Texture = sprite.Texture ;
    // effect.Vframes = sprite.Vframes ;
    // effect.Hframes = sprite.Hframes ;
    // effect.Frame = sprite.Frame ;
    // effect.FlipH = sprite.FlipH ;
  }

  private async void EndDash()
  {
    CanDash = false ;
    await ToSignal( GetTree().CreateTimer( DashDelay ), TimeOutEventName ) ;
    CanDash = true ;
  }

  private void OnDurationTimer_TimeOut()
  {
    Console.WriteLine( "EndDash" ) ;
    EndDash() ;
  }
}