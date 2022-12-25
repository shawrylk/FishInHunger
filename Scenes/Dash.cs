using Godot ;
using System ;
using Fish.Utilities ;

public class Dash : Spatial
{
  private const string DurationTimerPath = "DurationTimer" ;
  private Timer _durationTimer ;

  private bool _canDash = true ;
  // private PackedScene _dashEffectResource ;

  public bool CanDash
  {
    get => _canDash && ! IsDashing ;
    private set => _canDash = value ;
  }

  private const float DashDelay = 1f ;
  public bool IsDashing => ! _durationTimer.IsStopped() ;

  public override void _Ready()
  {
    _durationTimer = GetNode<Timer>( DurationTimerPath ) ;
    _durationTimer.OneShot = true ;
    _durationTimer.Connect( TimerExtensions.TimeOutEventName, this, nameof( OnDurationTimer_TimeOut ) ) ;
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
    await this.Wait( DashDelay ) ;
    CanDash = true ;
  }

  private void OnDurationTimer_TimeOut()
  {
    Console.WriteLine( "EndDash" ) ;
    EndDash() ;
  }
}