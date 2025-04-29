namespace AspnetCoreMvcFull.Events
{
  public interface IEventHandler<in T>
  {
    Task HandleAsync(T @event);
  }
}
