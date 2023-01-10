using System.Collections.Generic ;

namespace Fish.Scripts.Utilities
{
  public static class EnumerableExtensions
  {
    public static void AddThreadSafe<T>( this ICollection<T> list, T item, int maxCount = int.MaxValue )
    {
      lock ( list ) {
        if ( list.Count < maxCount ) list.Add( item ) ;
      }
    }

    public static void RemoveThreadSafe<T>( this ICollection<T> list, T item )
    {
      lock ( list ) {
        list.Remove( item ) ;
      }
    }
  }
}