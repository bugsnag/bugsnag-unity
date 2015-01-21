
public class ExampleCrash {

    public ExampleCrash() {};

    public void Crash() {
        throw new RuntimeException("from java");
    }

    public void CrashBackgroundThread() {
        new Thread(new Runnable() {
            public void run() {
                throw new RuntimeException("from java background thread");
            }
        }).start();
    }
}
