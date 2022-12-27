using System.Threading.Tasks ;
using Godot ;

namespace Fish.Scripts.Utilities
{
  public static class TimerExtensions
  {
    public const string TimeOutEventName = "timeout" ;

    public static async Task Wait( this Node node, float time )
    {
      await node.ToSignal( node.GetTree().CreateTimer( time ), TimeOutEventName ) ;
    }
  }
}