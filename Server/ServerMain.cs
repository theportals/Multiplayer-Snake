

namespace Server;

class ServerMain
{
    public static int Main(string[] args)
    {
        if (readArgs(args, out ushort port))
        {
            if (MessageQueueServer.instance.initialize(port))
            {
                Console.WriteLine($"Server listening on port {port}");
                startServer();
                MessageQueueServer.instance.shutdown();
            }
            else
            {
                Console.WriteLine("Failed to initialize MessageQueueServer");
            }
        }
        else
        {
            Console.WriteLine("Invalid port.");
            Console.WriteLine("Example usage: server --port 3000");
        }

        return 0;
    }


    /// <summary>
    /// Verify the command line port parameter is correct and if so, return the specified port
    /// </summary>
    private static bool readArgs(string[] args, out ushort port)
    {
        Predicate<string> PortParam = (arg => arg == "-p" || arg == "--port" || arg == "-port");

        port = 0;
        var valid = true;
        if (args.Length != 2)
        {
            valid = false;
        }
        else if (!PortParam(args[0].ToLower()))
        {
            valid = false;
        }
        else
        {
            if (!ushort.TryParse(args[1], out port))
            {
                valid = false;
            }
        }

        return valid;
    }

    private static void startServer()
    {
        var SIMULATION_UPDATE_RATE_MS = TimeSpan.FromMilliseconds(33);

        var model = new GameModel();
        bool running = model.initialize();

        var previousTime = DateTime.Now;
        while (running)
        {
            // work out the elapsed time
            var currentTime = DateTime.Now;
            var elapsedTime = currentTime - previousTime;
            previousTime = currentTime;

            // If we are running faster than the simulation update rate, then go to sleep
            // for a bit so we don't burn up the CPU unnecessarily.
            var sleepTime = SIMULATION_UPDATE_RATE_MS - elapsedTime;
            if (sleepTime > TimeSpan.Zero)
            {
                //Console.WriteLine("Sleep: {0}", sleepTime.TotalMilliseconds);
                Thread.Sleep(sleepTime);
            }

            // Now, after having slept for a bit, now compute the elapsed time and perform
            // the game model update.
            elapsedTime += (sleepTime > TimeSpan.Zero ? sleepTime : TimeSpan.Zero);
            model.update(elapsedTime);
        }

        model.shutdown();
    }
}