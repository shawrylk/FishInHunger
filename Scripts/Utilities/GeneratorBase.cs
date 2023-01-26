using System ;
using System.ComponentModel ;
using System.Reactive ;
using System.Reactive.Concurrency ;
using System.Reactive.Linq ;

namespace Fish.Scripts.Utilities
{
  public class GeneratorBase
  {
    public IObservable<EventPattern<DoWorkEventArgs>> GetObservableEvent( Type argumentType )
    {
      return Observable.FromEventPattern<DoWorkEventHandler, DoWorkEventArgs>(
          e => DoWorkEvent += e,
          e => DoWorkEvent -= e )
        .ObserveOn( Scheduler.CurrentThread )
        .Where( args => args.EventArgs.Argument.GetType() == argumentType ) ;
    }

    protected void AddNewWork( object workArgument )
    {
      DoWorkEvent?.Invoke( this, new DoWorkEventArgs( workArgument ) ) ;
    }

    public event DoWorkEventHandler DoWorkEvent ;
  }
}