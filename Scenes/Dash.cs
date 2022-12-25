using Godot ;
using System ;

public class Dash : Spatial
{
  private const string DurationTimerPath = "DurationTimer" ;
  private const string TimeOutEventName = "timeout" ;
  private Timer _durationTimer ;
  private bool _canDash = true ;
  // private PackedScene _dashEffectResource ;

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
    base._Ready() ;
  }

  public void StartDash( float duration )
  {
    _durationTimer.WaitTime = duration ;
    _durationTimer.Start() ;
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