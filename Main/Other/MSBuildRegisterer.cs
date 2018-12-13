using Microsoft.Build.Locator;
using System;
using System.Linq;
using System.Threading;

namespace Main.Other
{
    public static class MSBuildRegisterer
    {
        private static long _registered = 0L;

        public static void RegisterDefaultOnce(
            )
        {
            if(Interlocked.Exchange(ref _registered, 1L) != 0L)
            {
                return;
            }


            //var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            //var instance = visualStudioInstances.Length == 1
            //    // If there is only one instance of MSBuild on this machine, set that as the one to use.
            //    ? visualStudioInstances[0]
            //    // Handle selecting the version of MSBuild you want to use.
            //    : SelectVisualStudioInstance(visualStudioInstances);

            //Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            //// NOTE: Be sure to register an instance with the MSBuildLocator 
            ////       before calling MSBuildWorkspace.Create()
            ////       otherwise, MSBuildWorkspace won't MEF compose.
            //MSBuildLocator.RegisterInstance(instance);

            MSBuildLocator.RegisterDefaults();
        }


        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }

    }

}
