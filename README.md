# NetEncryptorWithRoslyn
Example to demonstrate the problem of hosting class not visible

When using C# scripting with a host class together with NetEncryptor, 
the program will terminate with an error pointing out that the methods and members 
in the host class are not existing in the current context.

In the case of this example, the host class has a public function Square(), the calling of which in a script produces the following error:

error CS7012: The name 'Square' does not exist in the current context (are you missing a reference to assembly 'TestScriptingApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=94fdfcf678734431'?)"

The use of a host class with Scripting is explained here:

https://blogs.msdn.microsoft.com/cdndevs/2015/12/01/adding-c-scripting-to-your-development-arsenal-part-1/

The example is this repository is based on the Bootstrap sample project from Infralution and the scripting test app is based on the code from the blog above.

Set the TestScriptingApp project as startup project and build and run, this should produce the answer 16 in the console if Scripting is working.

Then set the Bootstrap project as startup project and the error message will appear.
