namespace AspnetCoreMvcFull.Events
{
  public class EventPublisher : IEventPublisher
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IServiceProvider serviceProvider, ILogger<EventPublisher> logger)
    {
      _serviceProvider = serviceProvider;
      _logger = logger;
    }

    public async Task PublishAsync<T>(T @event)
    {
      using var scope = _serviceProvider.CreateScope();
      var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();

      foreach (var handler in handlers)
      {
        try
        {
          await handler.HandleAsync(@event);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error handling event {EventType} by handler {HandlerType}",
              typeof(T).Name, handler.GetType().Name);
        }
      }
    }
  }
}
