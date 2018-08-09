using BugsnagUnity.Payload;

namespace BugsnagUnity
{
  public delegate void Middleware(Report report);

  public interface IClient
  {
    IConfiguration Configuration { get; }

    IBreadcrumbs Breadcrumbs { get; }

    ISessionTracker SessionTracking { get; }

    User User { get; }

    void Send(IPayload payload);

    Metadata Metadata { get; }

    void BeforeNotify(Middleware middleware);

    void Notify(System.Exception exception);

    void Notify(System.Exception exception, Middleware callback);

    void Notify(System.Exception exception, Severity severity);

    void Notify(System.Exception exception, Severity severity, Middleware callback);
  }
}
