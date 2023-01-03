using System.Diagnostics ;
using Godot ;

namespace Fish.Scripts.Utilities
{
  public static class DebugUtilities
  {
    public static Stopwatch StartStopwatch()
    {
      var sw = new Stopwatch() ;
      sw.Start() ;
      return sw ;
    }

    public static void StopStopwatch( this Stopwatch sw )
    {
      sw.Stop() ;
      GD.Print( $"{sw.ElapsedMilliseconds} ms" ) ;
    }

    public static DisposeExtensions.DisposableAction<Stopwatch> StartDisposableStopwatch()
    {
      var sw = new Stopwatch() ;
      sw.Start() ;
      return new DisposeExtensions.DisposableAction<Stopwatch>( StopStopwatch, sw ) ;
    }
  }
}