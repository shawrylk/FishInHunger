using System ;
using System.Collections.Generic ;
using System.ComponentModel ;
using System.Reactive ;
using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Runtime.CompilerServices ;

namespace Fish.Scripts.Utilities
{
  public class BindableBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged ;

    protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
    {
      PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) ) ;
    }

    protected bool SetField<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
    {
      if ( EqualityComparer<T>.Default.Equals( field, value ) ) return false ;
      field = value ;
      OnPropertyChanged( propertyName ) ;
      return true ;
    }

    public IObservable<EventPattern<PropertyChangedEventArgs>> GetObservablePropertyChanged( string propertyName )
    {
      return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
          e => PropertyChanged += e,
          e => PropertyChanged -= e )
        .ObserveOn( Scheduler.CurrentThread )
        .Where( args => args.EventArgs.PropertyName == propertyName ) ;
    }
  }
}