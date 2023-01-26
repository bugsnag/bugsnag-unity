# Unity Example App

This Application was made to demonstrate various features of the BugSnag Unity notifier. 

## Features including:
<ul>
    <li>Handled errors and exceptions: Events which can be handled gracefully can also be reported to BugSnag</li>
    <li>Crashes: Events which terminate the app are sent to Bugsnag automatically. Reopen the app after a crash to send reports.</li>
    <li>Android specific crashes: Native, segfaults, C++, JVM & ANR exceptions. Reopen the app after a crash to send reports</li>
    <li>Cocoa sepecific crashes: Native, C++, OOM & App Hangs. Reopen the app after a crash to send reports</li>
</ul>


## Running the app

<ol>
    <li>Clone the repo </li> 
```
git clone git@github.com:bugsnag/bugsnag-unity.git --recursive
```
    <li>Once the project has opened in Unity</li> 
    <li>Navigate to `Windows>Bugsnag>Configuration`</li>
    <li>Enter your Bugsnag project API Key under the basic Configuration section of the package window</li>
    <li>Press `Play` in the editor</li>
    <li>Once loaded, click any of the error buttons</li>
    <li>Confirm whether the BugSnag dashboard has received the error.</li>
</ol>

For more configuration and full information please read the documentation at 
https://docs.bugsnag.com/platforms/unity/configuration-options/

