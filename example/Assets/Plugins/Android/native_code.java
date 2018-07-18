public class BugsnagCrash {
  public BugsnagCrash() {};

  public void Crash() {
    throw new RuntimeException("from java");
  }

  public void BackgroundCrash() {
    new Thread(new Runnable() {
      public void run() {
        throw new RuntimeException("from java background thread");
      }
    }).start();
  }
}
