using System ;

namespace Fish.Scripts.Utilities
{
  public static class DisposeExtensions
  {
    public class DisposableAction<T> : IDisposable
    {
      private readonly Action<T> _action ;
      private readonly T _param ;

      public DisposableAction( Action<T> action, T param )
      {
        _action = action ;
        _param = param ;
      }

      public void Dispose()
      {
        _action.Invoke( _param ) ;
      }
    }
  }
}