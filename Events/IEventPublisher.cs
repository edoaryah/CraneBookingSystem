namespace AspnetCoreMvcFull.Events
{
  public interface IEventPublisher
  {
    Task PublishAsync<T>(T @event);
  }
}
