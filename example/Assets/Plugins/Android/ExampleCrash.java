import java.lang.RuntimeException;

class ExampleCrash {

    public void Crash() {
        throw new RuntimeException("crash from java foreground");
    }

    public void CrashBackgroundThread() {

        new Thread(new Runnable() {
            public void run() {
                throw new RuntimeException("crash from java background");
            }
        }).start();
    }

}
