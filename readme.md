# Blazr.Demo.EditForm

This Repo contains the demo solution for Form locking and Navigation Locking.

If you are Net7.0 use the Net7 branch.  The documentation is a work in progress at the moment but the solution demonstrates using the new NavigationManager features and NavigationLock component.  The Net6 and master branches are the old Net6 code version.

If you used the old Net6.0 solution, and want to upgrade to Net7, I suggest you rename the solution's `NavigationLock` to `BlazrNavigationLock` before ungrading to fix component name conflicts you will get.  The MS Blazor Net7 solution uses patterns from the solution and the same component name!  

The pre Net7.0 issue is on the official aspnetcore github site here.

[[Blazor] Add support for confirming navigations #40149](https://github.com/dotnet/aspnetcore/issues/40149)

The detailed Net6 articles can be found here:

[Building Edit forms](https://shauncurtis.github.io/articles/Building-Edit-Forms.html)

The Net 6 demo version of this code can be seen here:

[Blazr.Demo Azure Site](https://blazr-demo.azurewebsites.net/)

Updates:
 - 12-Sep-2022 - Added Support for Hot Reload
 - 14-Nov-2022 - work on the Net7 version and Docs update
