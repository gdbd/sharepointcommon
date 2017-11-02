To run test is no need to use any special site, test creates all what they need automatically on a blank site. 
At now tests code open site located on 80 port of local machine and works in it. 
After tests run, all test data/infrastructure deletes.
To run tests you may use **Resharper** runner directly from Visual Studio, or any other tool.  
The main thing is run tests in 64-bit process.

If you test modified version of library, make sure that original retracted from GAC!

For a contributors of library is exist temp sign key, located in the project directory and named **temp.snk**

Original sign key protected by password and it is only project coordinator knows password.